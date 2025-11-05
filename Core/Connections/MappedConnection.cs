using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using MQContract.Middleware;
using System.Diagnostics;
using System.Reflection;

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
        private readonly SemaphoreSlim publishLock = new(1, 1);
        ValueTask<PingResult> IContractConnection.PingAsync()
        {
            using var scope = SetScope();
            Logger?.LogDebug("Attempting to call Ping against an underlying service connection");
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
            publishLock.Dispose();
            await base.InternalDisposeAsync();
        }

        private new async ValueTask<ServiceConnectionList.ServiceConnection> GetConnectionsAsync(string channel, Type messageType, MessageHeader messageHeader)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Locating a connection for {Channel}, {MessageType} and {HeaderKeys}", channel, messageType, string.Join(',', messageHeader.Keys));
            var connections = await base.GetConnectionsAsync(channel, messageType, messageHeader);
            if (connections.Count()>1)
            {
                Logger?.LogError("Located more than 1 connection for {Channel}, {MessageType} and {HeaderKeys}", channel, messageType, string.Join(',', messageHeader.Keys));
                throw new TooManyConnectionMatchesException();
            }
            return connections.First();
        }

        private new async ValueTask<(ServiceConnectionList.ServiceConnection connections, string channel)> GetConnectionsAsync<T>(string? channel, ChannelMapper.MapTypes mapTypes)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Locating a connection for {Channel}, {T} and {MapType}", channel, typeof(T), mapTypes);
            (var connections, channel) = await base.GetConnectionsAsync<T>(channel, mapTypes);
            if (connections.Count()>1)
            {
                Logger?.LogError("Located more than 1 connection for {Channel}, {T} and {MapType}", channel, typeof(T), mapTypes);
                throw new TooManyConnectionMatchesException();
            }
            return (connections.First(), channel);
        }

        #region PubSub
        protected override async ValueTask<ISubscription> CreateSubscriptionAsync<T>(Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<T>? messageFilters, bool synchronous, CancellationToken cancellationToken)
        {
            (var connection, channel) = await GetConnectionsAsync<T>(channel, ChannelMapper.MapTypes.PublishSubscription);
            return await CreateSubscriptionAsync<T>(
                GetMessageFactory<T>(ignoreMessageHeader),
                connection.MessageServiceConnection,
                messageReceived,
                errorReceived,
                channel,
                group,
                synchronous,
                connection.ServiceConnectionName,
                messageFilters,
                cancellationToken
            );
        }

        async ValueTask<TransmissionResult> IContractConnection.PublishAsync<T>(T message, string? channel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Publishing message {T} on {Channel}", typeof(T), channel);
            using var activity = StartActivity(Constants.PublishActivityName);
            var serviceMessage = await ProduceServiceMessageAsync<T>(
                ChannelMapper.MapTypes.Publish,
                GetMessageFactory<T>(),
                message,
                false,
                activity,
                maxMessageSize: MaxMessageBodySize,
                channel: channel, 
                messageHeader: messageHeader
            );
            var serviceConnection = await GetConnectionsAsync(serviceMessage.Channel, typeof(T), serviceMessage.Header);
            OpenTelemetryMiddleware.AssignConnectionType(activity, serviceConnection.MessageServiceConnection, serviceConnection.ServiceConnectionName);
            return await PublishMessageAsync<T>(publishLock, serviceMessage, serviceConnection.MessageServiceConnection, activity, serviceConnection.ServiceConnectionName, cancellationToken);
        }

        async ValueTask<IEnumerable<TransmissionResult>> IContractConnection.BulkPublishAsync<T>(IEnumerable<(T message, MessageHeader? messageHeader)> messages, string? channel, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Bulk Publishing messages {T} on {Channel}", typeof(T), channel);
            using var activity = StartActivity(Constants.BulkPublishActivityName);
            var serviceMessages = await
            messages.WhenAll(m =>
                    ProduceServiceMessageAsync<T>(
                        ChannelMapper.MapTypes.Publish,
                        GetMessageFactory<T>(),
                        m.message,
                        false,
                        activity,
                        maxMessageSize: MaxMessageBodySize,
                        channel: channel, 
                        messageHeader: m.messageHeader
                    )
                );
            var serviceConnection = await GetConnectionsAsync(serviceMessages.First().Channel, typeof(T), serviceMessages.First().Header);
            OpenTelemetryMiddleware.AssignConnectionType(activity, serviceConnection.MessageServiceConnection, serviceConnection.ServiceConnectionName);
            await publishLock.WaitAsync(cancellationToken);
            var result = await BulkPublishAsync<T>(serviceMessages, serviceConnection.MessageServiceConnection, activity, cancellationToken, connectionName: serviceConnection.ServiceConnectionName);
            publishLock.Release();
            activity?.SetStatus(result.Any(r => r.IsError) ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            activity?.Stop();
            return result;
        }
        #endregion

        #region QueryResponse
        async ValueTask<QueryResult<R>> IContractConnection.QueryAsync<Q, R>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Executing QueryResponse of {Q}, expecting {R} on {Channel} with {ResponseChannel}", typeof(Q), typeof(R), channel, responseChannel);
            using var activity = StartActivity(Constants.PublishQueryActivityName);
            var serviceMessage = await ProduceServiceMessageAsync<Q>(
                ChannelMapper.MapTypes.Query, 
                GetMessageFactory<Q>(), 
                message, 
                false, 
                activity, 
                maxMessageSize: MaxMessageBodySize,
                channel: channel, 
                messageHeader: messageHeader
            );
            var serviceConnection = await GetConnectionsAsync(serviceMessage.Channel, typeof(Q), serviceMessage.Header);
            OpenTelemetryMiddleware.AssignConnectionType(activity, serviceConnection.MessageServiceConnection, serviceConnection.ServiceConnectionName);
            return await ExecuteQueryAsync<Q, R>(serviceConnection.MessageServiceConnection, serviceMessage, activity, timeout: timeout, responseChannel: responseChannel, connectionName: serviceConnection.ServiceConnectionName, cancellationToken: cancellationToken);
        }

        private static readonly MethodInfo QueryMethod = typeof(IContractConnection).GetMethods()
            .First(method => Equals(method.Name, nameof(IContractConnection.QueryAsync)) && method.GetGenericArguments().Length==2);
        async ValueTask<QueryResult<object>> IContractConnection.QueryAsync<Q>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader,
            CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Attempting to get response type for QueryResponse for {Q} on {Channel} with {ResponseChannel}", typeof(Q), channel, responseChannel);
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var responseType = (typeof(Q).GetCustomAttribute<QueryMessageAttribute>(false)?.ResponseType)??throw new UnknownResponseTypeException("ResponseType", typeof(Q));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            Logger?.LogInformation("Obtained {ResponseType} for QueryResponse for {Q} on {Channel} with {ResponseChannel}", responseType, typeof(Q), channel, responseChannel);
            var methodInfo = QueryMethod.MakeGenericMethod(typeof(Q), responseType!);
            try
            {
                return Utility.ConvertResultFromObject(await Utility.InvokeMethodAsync(
                    methodInfo,
                    this,
                    [
                        message,
                        timeout,
                        channel,
                        responseChannel,
                        messageHeader,
                        cancellationToken
                    ])
                )!;
            }
            catch (TimeoutException)
            {
                throw new QueryTimeoutException();
            }
        }

        protected override async ValueTask<ISubscription> ProduceSubscribeQueryResponseAsync<Q, R>(Func<IReceivedMessage<Q>, ValueTask<QueryResponseMessage<R>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Producing QueryResponse Subscription for {Q} responding with {R} on {Channel} in {Group}", typeof(Q), typeof(R), channel, group);
            var queryMessageFactory = GetMessageFactory<Q>(ignoreMessageHeader);
            var responseMessageFactory = GetMessageFactory<R>();
            (var serviceConnection, channel) = await GetConnectionsAsync<Q>(channel, ChannelMapper.MapTypes.QuerySubscription);
            return await CreateSubscriptionAsync<Q, R>(queryMessageFactory, responseMessageFactory, serviceConnection.MessageServiceConnection, messageReceived, errorReceived, channel, group, synchronous, serviceConnection.ServiceConnectionName, cancellationToken);
        }
        #endregion
    }
}
