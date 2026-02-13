using Microsoft.Extensions.Logging;
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
    internal class MappedConnection(IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null) :
        AMappableConnection<IMappedContractConnection>(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, logger, channelMapper),
        IMappedContractConnection
    {
        ValueTask<PingResult> IContractConnection.PingAsync()
        {
            using var scope = SetScope();
            BaseLog.PingServiceConnection(Logger);  
            var connections = FullList.Select(c => c.MessageServiceConnection).OfType<IPingableMessageServiceConnection>();
            return connections.Count() switch
            {
                0 => throw new PingNotSupportedException(),
                1 => connections.First().PingAsync(),
                _ => throw new TooManyConnectionMatchesException()
            };
        }

        protected override async ValueTask InternalDisposeAsync()
        {
            await base.InternalDisposeAsync();
        }

        private readonly record struct GetConnectionResult(ServiceConnectionList.ServiceConnection Connection, string Channel);

        private async ValueTask<ServiceConnectionList.ServiceConnection> GetConnectionAsync(string channel, Type messageType, MessageHeader messageHeader)
        {
            using var scope = SetScope();
            var connections = await base.GetConnectionsAsync(channel, messageType, messageHeader);
            if (connections.Count()>1)
            {
                MappableConnectionLog.LocatedTooManyConnections(Logger, channel, messageType, string.Join(',', messageHeader.Keys));
                throw new TooManyConnectionMatchesException();
            }
            return connections.First();
        }

        private async ValueTask<GetConnectionResult> GetConnectionAsync<TMessage>(string? channel, ChannelMapper.MapTypes mapTypes)
        {
            using var scope = SetScope();
            MappableConnectionLog.LocatingConnectionsForMapType(Logger, channel, typeof(TMessage), mapTypes);
            var connections = await base.GetConnectionsAsync<TMessage>(channel, mapTypes);
            if (connections.Connections.Count()>1)
            {
                MappableConnectionLog.LocatedTooManyConnectionsForMapType(Logger, channel, typeof(TMessage), mapTypes);
                throw new TooManyConnectionMatchesException();
            }
            return new(connections.Connections.First(), connections.Channel);
        }

        #region PubSub
        protected override async ValueTask<ISubscription> CreateSubscriptionAsync<TMessage>(Func<IReceivedMessage<TMessage>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, bool synchronous, CancellationToken cancellationToken)
        {
            var connection = await GetConnectionAsync<TMessage>(channel, ChannelMapper.MapTypes.PublishSubscription);
            return await CreateSubscriptionAsync<TMessage>(
                GetMessageFactory<TMessage>(ignoreMessageHeader),
                connection.Connection.MessageServiceConnection,
                messageReceived,
                errorReceived,
                connection.Channel,
                group,
                synchronous,
                connection.Connection.ServiceConnectionName,
                messageFilters,
                cancellationToken
            );
        }

        async ValueTask<TransmissionResult> IContractConnection.PublishAsync<TMessage>(TMessage message, string? channel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            PubSubLog.PublishingMessage(Logger, typeof(TMessage), channel);
            using var activity = StartActivity(Constants.PublishActivityName);
            var serviceMessage = await ProduceServiceMessageAsync<TMessage>(
                ChannelMapper.MapTypes.Publish,
                GetMessageFactory<TMessage>(),
                message,
                false,
                activity,
                maxMessageSize: MaxMessageBodySize,
                channel: channel,
                messageHeader: messageHeader
            );
            var serviceConnection = await GetConnectionAsync(serviceMessage.Channel, typeof(TMessage), serviceMessage.Header);
            OpenTelemetryMiddleware.AssignConnectionType(activity, serviceConnection.MessageServiceConnection, serviceConnection.ServiceConnectionName);
            return await PublishMessageAsync<TMessage>(serviceMessage, serviceConnection.MessageServiceConnection, activity, serviceConnection.ServiceConnectionName, cancellationToken);
        }

        async ValueTask<IEnumerable<TransmissionResult>> IContractConnection.BulkPublishAsync<TMessage>(IEnumerable<(TMessage message, MessageHeader? messageHeader)> messages, string? channel, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            PubSubLog.BulkPublishingMessage(Logger, typeof(TMessage), channel);
            using var activity = StartActivity(Constants.BulkPublishActivityName);
            var serviceMessages = await
            messages.WhenAll(m =>
                    ProduceServiceMessageAsync<TMessage>(
                        ChannelMapper.MapTypes.Publish,
                        GetMessageFactory<TMessage>(),
                        m.message,
                        false,
                        activity,
                        maxMessageSize: MaxMessageBodySize,
                        channel: channel,
                        messageHeader: m.messageHeader
                    )
                );
            var serviceConnection = await GetConnectionAsync(serviceMessages.First().Channel, typeof(TMessage), serviceMessages.First().Header);
            OpenTelemetryMiddleware.AssignConnectionType(activity, serviceConnection.MessageServiceConnection, serviceConnection.ServiceConnectionName);
            var result = await BulkPublishAsync<TMessage>(serviceMessages, serviceConnection.MessageServiceConnection, activity, cancellationToken, connectionName: serviceConnection.ServiceConnectionName);
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
            using var activity = StartActivity(Constants.PublishQueryActivityName);
            var serviceMessage = await ProduceServiceMessageAsync<TQuery>(
                ChannelMapper.MapTypes.Query,
                GetMessageFactory<TQuery>(),
                message,
                false,
                activity,
                maxMessageSize: MaxMessageBodySize,
                channel: channel,
                messageHeader: messageHeader
            );
            var serviceConnection = await GetConnectionAsync(serviceMessage.Channel, typeof(TQuery), serviceMessage.Header);
            OpenTelemetryMiddleware.AssignConnectionType(activity, serviceConnection.MessageServiceConnection, serviceConnection.ServiceConnectionName);
            return await ExecuteQueryAsync<TQuery, TQueryResponse>(serviceConnection.MessageServiceConnection, serviceMessage, activity, timeout: timeout, responseChannel: responseChannel, connectionName: serviceConnection.ServiceConnectionName, cancellationToken: cancellationToken);
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
            var connection = await GetConnectionAsync<TQuery>(channel, ChannelMapper.MapTypes.QuerySubscription);
            return await CreateSubscriptionAsync<TQuery, TQueryResponse>(queryMessageFactory, responseMessageFactory, connection.Connection.MessageServiceConnection, messageReceived, errorReceived, connection.Channel, group, synchronous, connection.Connection.ServiceConnectionName, messageFilter, cancellationToken);
        }
        #endregion
    }
}
