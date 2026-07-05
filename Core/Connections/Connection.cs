using Microsoft.Extensions.Logging;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Service;
using MQContract.Logging;
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

        ValueTask<TransmissionResult> IContractConnection.PublishAsync<TMessage>(TMessage message, string? channel, MessageHeader? messageHeader, CancellationToken cancellationToken)
            => ((IContractConnection)this).PublishAsync(new TransmissionMessage<TMessage>(message, Header: messageHeader), channel, cancellationToken);
        ValueTask<TransmissionResult> IContractConnection.PublishAsync<TMessage>(TMessage message, string? channel, CancellationToken cancellationToken)
            => ((IContractConnection)this).PublishAsync(new TransmissionMessage<TMessage>(message), channel, cancellationToken);
        async ValueTask<TransmissionResult> IContractConnection.PublishAsync<TMessage>(TransmissionMessage<TMessage> message, string? channel, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logs.Publishing.PublishingMessage(Logger, typeof(TMessage), channel);
            using var activity = StartActivity(Constants.PublishActivityName, serviceConnection: serviceConnection);
            var serviceMessage = await ProduceServiceMessageAsync<TMessage>(
                ChannelMapper.MapTypes.Publish,
                GetMessageFactory<TMessage>(),
                message,
                false,
                activity,
                maxMessageSize: serviceConnection.MaxMessageBodySize,
                channel: channel
            );
            return await PublishMessageAsync<TMessage>(serviceMessage, serviceConnection, activity, null, cancellationToken);
        }

        ValueTask<IEnumerable<TransmissionResult>> IContractConnection.BulkPublishAsync<TMessage>(IEnumerable<(TMessage message, MessageHeader? messageHeader)> messages, string? channel, CancellationToken cancellationToken)
            => ((IContractConnection)this).BulkPublishAsync(messages.Select(m => new TransmissionMessage<TMessage>(m.message, Header: m.messageHeader)), channel, cancellationToken);
        ValueTask<IEnumerable<TransmissionResult>> IContractConnection.BulkPublishAsync<TMessage>(IEnumerable<TMessage> messages, string? channel, CancellationToken cancellationToken)
            => ((IContractConnection)this).BulkPublishAsync(messages.Select(m => new TransmissionMessage<TMessage>(m)), channel, cancellationToken);
        async ValueTask<IEnumerable<TransmissionResult>> IContractConnection.BulkPublishAsync<TMessage>(IEnumerable<TransmissionMessage<TMessage>> messages, string? channel, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logs.Publishing.BulkPublishingMessages(Logger, typeof(TMessage), channel);
            using var activity = StartActivity(Constants.BulkPublishActivityName, serviceConnection: serviceConnection);
            activity?.SetTag(Constants.BulkPublishCountTag, messages.Count());
            var serviceMessages = await
                messages.WhenAll(m =>
                    ProduceServiceMessageAsync<TMessage>(
                        ChannelMapper.MapTypes.Publish,
                        GetMessageFactory<TMessage>(),
                        m,
                        false,
                        activity,
                        maxMessageSize: serviceConnection.MaxMessageBodySize,
                        channel: channel
                    )
                );
            var result = await BulkPublishAsync<TMessage>(serviceMessages, serviceConnection, activity, cancellationToken);
            activity?.SetStatus(result.Any(r => r.IsError) ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            activity?.Stop();
            return result;
        }
        #endregion

        #region QueryResponse
        ValueTask<QueryResult<TQueryResponse>> IContractConnection.QueryAsync<TQuery, TQueryResponse>(TQuery message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
            => ((IContractConnection)this).QueryAsync<TQuery, TQueryResponse>(new TransmissionMessage<TQuery>(message, Header: messageHeader), timeout, channel, responseChannel, cancellationToken);
        ValueTask<QueryResult<TQueryResponse>> IContractConnection.QueryAsync<TQuery, TQueryResponse>(TQuery message, TimeSpan? timeout, string? channel, string? responseChannel, CancellationToken cancellationToken)
            => ((IContractConnection)this).QueryAsync<TQuery, TQueryResponse>(new TransmissionMessage<TQuery>(message), timeout, channel, responseChannel, cancellationToken);
        async ValueTask<QueryResult<TQueryResponse>> IContractConnection.QueryAsync<TQuery, TQueryResponse>(TransmissionMessage<TQuery> message, TimeSpan? timeout, string? channel, string? responseChannel, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logs.Publishing.ExecutingQuery(Logger, typeof(TQuery), typeof(TQueryResponse), channel, responseChannel);
            using var activity = StartActivity(Constants.PublishQueryActivityName, serviceConnection: serviceConnection);
            OpenTelemetryMiddleware.TagEventID(activity, EventIds.Publishing.ExecutingQuery);
            var serviceMessage = await ProduceServiceMessageAsync<TQuery>(
                ChannelMapper.MapTypes.Query,
                GetMessageFactory<TQuery>(),
                message,
                false,
                activity,
                maxMessageSize: serviceConnection.MaxMessageBodySize,
                channel: channel
            );
            return await ExecuteQueryAsync<TQuery, TQueryResponse>(serviceConnection, serviceMessage, activity, timeout: timeout, responseChannel: responseChannel, cancellationToken: cancellationToken);
        }

        ValueTask<QueryResult<object>> IContractConnection.QueryAsync<TQuery>(TQuery message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
            => ((IContractConnection)this).QueryAsync<TQuery>(new TransmissionMessage<TQuery>(message, Header: messageHeader), timeout, channel, responseChannel, cancellationToken);
        ValueTask<QueryResult<object>> IContractConnection.QueryAsync<TQuery>(TQuery message, TimeSpan? timeout, string? channel, string? responseChannel, CancellationToken cancellationToken)
            => ((IContractConnection)this).QueryAsync<TQuery>(new TransmissionMessage<TQuery>(message), timeout, channel, responseChannel, cancellationToken);
        async ValueTask<QueryResult<object>> IContractConnection.QueryAsync<TQuery>(TransmissionMessage<TQuery> message, TimeSpan? timeout, string? channel, string? responseChannel, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logs.Pipeline.ExtractingQueryResponseType(Logger, typeof(TQuery), channel, responseChannel);
            return await messageContext.ExecuteQuery<TQuery>(this, message, timeout, channel, responseChannel, cancellationToken);
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
