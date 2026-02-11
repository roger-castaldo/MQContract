using Microsoft.Extensions.Logging;
using MQContract.Extensions;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Service;
using MQContract.Loggers;
using MQContract.Messages;
using MQContract.Middleware;
using System.Diagnostics;

namespace MQContract.Connections
{
    internal class Connection(IMessageServiceConnection serviceConnection,
        IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null) :
        AConnection<IContractedConnection>(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, logger, channelMapper),
        IContractedConnection
    {
        ValueTask<PingResult> IContractConnection.PingAsync()
            => (serviceConnection is IPingableMessageServiceConnection pingableService ? pingableService.PingAsync() : throw new PingNotSupportedException());

        protected override ConnectionHealthCheck? ProduceConnectionHealthCheck()
            => new(connection: serviceConnection);

        protected override async ValueTask CloseAsync()
            => await (serviceConnection?.CloseAsync()??ValueTask.CompletedTask);

        protected override async ValueTask InternalDisposeAsync()
        {
            if (serviceConnection is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(true);
            else if (serviceConnection is IDisposable disposable)
                disposable.Dispose();
        }

        #region PubSub
        protected override ValueTask<ISubscription> CreateSubscriptionAsync<TMessage>(Func<IReceivedMessage<TMessage>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, bool synchronous, CancellationToken cancellationToken)
            => CreateSubscriptionAsync<TMessage>(
                GetMessageFactory<TMessage>(ignoreMessageHeader),
                serviceConnection,
                messageReceived,
                errorReceived,
                channel,
                group,
                synchronous,
                null,
                messageFilters,
                cancellationToken
            );

        async ValueTask<TransmissionResult> IContractConnection.PublishAsync<TMessage>(TMessage message, string? channel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            PubSubLog.PublishingMessage(Logger, typeof(TMessage), channel);
            using var activity = StartActivity(Constants.PublishActivityName, serviceConnection: serviceConnection);
            var serviceMessage = await ProduceServiceMessageAsync<TMessage>(
                ChannelMapper.MapTypes.Publish,
                GetMessageFactory<TMessage>(),
                message,
                false,
                activity,
                maxMessageSize: serviceConnection.MaxMessageBodySize,
                channel: channel,
                messageHeader: messageHeader
            );
            return await PublishMessageAsync<TMessage>(serviceMessage, serviceConnection, activity, null, cancellationToken);
        }

        async ValueTask<IEnumerable<TransmissionResult>> IContractConnection.BulkPublishAsync<TMessage>(IEnumerable<(TMessage message, MessageHeader? messageHeader)> messages, string? channel, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            PubSubLog.BulkPublishingMessage(Logger, typeof(TMessage), channel);
            using var activity = StartActivity(Constants.BulkPublishActivityName, serviceConnection: serviceConnection);
            activity?.SetTag(Constants.BulkPublishCountTag, messages.Count());
            var serviceMessages = await
                messages.WhenAll(m =>
                    ProduceServiceMessageAsync<TMessage>(
                        ChannelMapper.MapTypes.Publish,
                        GetMessageFactory<TMessage>(),
                        m.message,
                        false,
                        activity,
                        maxMessageSize: serviceConnection.MaxMessageBodySize,
                        channel: channel,
                        messageHeader: m.messageHeader
                    )
                );
            var result = await BulkPublishAsync<TMessage>(serviceMessages, serviceConnection, activity, cancellationToken);
            activity?.SetStatus(result.Any(r => r.IsError) ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            activity?.Stop();
            return result;
        }
        #endregion

        #region QueryResponse
        async ValueTask<QueryResult<TQueryResponse>> IContractConnection.QueryAsync<TQuery, TQueryResponse>(TQuery message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            QueryResponseLog.ExecutingQuery(Logger, typeof(TQuery), typeof(TQueryResponse), channel, responseChannel);
            using var activity = StartActivity(Constants.PublishQueryActivityName, serviceConnection: serviceConnection);
            OpenTelemetryMiddleware.TagEventID(activity, QueryResponseLog.ExecutingQueryEventId);
            var serviceMessage = await ProduceServiceMessageAsync<TQuery>(
                ChannelMapper.MapTypes.Query,
                GetMessageFactory<TQuery>(),
                message,
                false,
                activity,
                maxMessageSize: serviceConnection.MaxMessageBodySize,
                channel: channel,
                messageHeader: messageHeader
            );
            return await ExecuteQueryAsync<TQuery, TQueryResponse>(serviceConnection, serviceMessage, activity, timeout: timeout, responseChannel: responseChannel, cancellationToken: cancellationToken);
        }

        async ValueTask<QueryResult<object>> IContractConnection.QueryAsync<TQuery>(TQuery message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader,
            CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            QueryResponseLog.ExtractingQueryResponseType(Logger, typeof(TQuery), channel, responseChannel);
            return await messageContext.ExecuteQuery<TQuery>(this, message, timeout, channel, responseChannel, messageHeader, cancellationToken);
        }

        protected override async ValueTask<ISubscription> ProduceSubscribeQueryResponseAsync<TQuery, TQueryResponse>(Func<IReceivedMessage<TQuery>, ValueTask<QueryResponseMessage<TQueryResponse>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, MessageFilters<TQuery>? messageFilter, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            var queryMessageFactory = GetMessageFactory<TQuery>(ignoreMessageHeader);
            var responseMessageFactory = GetMessageFactory<TQueryResponse>();
            return await CreateSubscriptionAsync<TQuery, TQueryResponse>(
                queryMessageFactory,
                responseMessageFactory,
                serviceConnection,
                messageReceived,
                errorReceived,
                channel,
                group,
                synchronous,
                null,
                messageFilter,
                cancellationToken
            );
        }
        #endregion
    }
}
