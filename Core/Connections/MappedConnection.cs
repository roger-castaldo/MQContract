using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Extensions;
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
            Logger?.LogDebugChecked("Attempting to call Ping against an underlying service connection");
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

        private async ValueTask<ServiceConnectionList.ServiceConnection> GetConnectionAsync(string channel, Type messageType, MessageHeader messageHeader)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Locating a connection for {Channel}, {MessageType} and {HeaderKeys}", channel, messageType, string.Join(',', messageHeader.Keys));
            var connections = await base.GetConnectionsAsync(channel, messageType, messageHeader);
            if (connections.Count()>1)
            {
                Logger?.LogErrorChecked("Located more than 1 connection for {Channel}, {MessageType} and {HeaderKeys}", channel, messageType, string.Join(',', messageHeader.Keys));
                throw new TooManyConnectionMatchesException();
            }
            return connections.First();
        }

        private readonly record struct GetConnectionResult(ServiceConnectionList.ServiceConnection Connection, string Channel);

        private async ValueTask<GetConnectionResult> GetConnectionAsync<TMessage>(string? channel, ChannelMapper.MapTypes mapTypes)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Locating a connection for {Channel}, {TMessage} and {MapType}", channel, typeof(TMessage), mapTypes);
            var connections = await base.GetConnectionsAsync<TMessage>(channel, mapTypes);
            if (connections.Connections.Count()>1)
            {
                Logger?.LogErrorChecked("Located more than 1 connection for {Channel}, {TMessage} and {MapType}", channel, typeof(TMessage), mapTypes);
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
            Logger?.LogDebugChecked("Publishing message {TMessage} on {Channel}", typeof(TMessage), channel);
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
            return await PublishMessageAsync<TMessage>(publishLock, serviceMessage, serviceConnection.MessageServiceConnection, activity, serviceConnection.ServiceConnectionName, cancellationToken);
        }

        async ValueTask<IEnumerable<TransmissionResult>> IContractConnection.BulkPublishAsync<TMessage>(IEnumerable<(TMessage message, MessageHeader? messageHeader)> messages, string? channel, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Bulk Publishing messages {TMessage} on {Channel}", typeof(TMessage), channel);
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
            var result = await BulkPublishAsync<TMessage>(publishLock, serviceMessages, serviceConnection.MessageServiceConnection, activity, cancellationToken, connectionName: serviceConnection.ServiceConnectionName);
            activity?.SetStatus(result.Any(r => r.IsError) ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            activity?.Stop();
            return result;
        }
        #endregion

        #region QueryResponse
        async ValueTask<QueryResult<TQueryResponse>> IContractConnection.QueryAsync<TQuery, TQueryResponse>(TQuery message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Executing QueryResponse of {TQuery}, expecting {TQueryResponse} on {Channel} with {ResponseChannel}", typeof(TQuery), typeof(TQueryResponse), channel, responseChannel);
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

        private static readonly MethodInfo QueryMethod = typeof(IContractConnection).GetMethods()
            .First(method => Equals(method.Name, nameof(IContractConnection.QueryAsync)) && method.GetGenericArguments().Length==2);
        async ValueTask<QueryResult<object>> IContractConnection.QueryAsync<TQuery>(TQuery message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader,
            CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Attempting to get response type for QueryResponse for {TQuery} on {Channel} with {ResponseChannel}", typeof(TQuery), channel, responseChannel);
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var responseType = Utility.GetCustomAttribute<TQuery,QueryMessageAttribute>()?.ResponseType??throw new UnknownResponseTypeException("ResponseType", typeof(TQuery));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            Logger?.LogInformationChecked("Obtained {ResponseType} for QueryResponse for {TQuery} on {Channel} with {ResponseChannel}", responseType, typeof(TQuery), channel, responseChannel);
            var methodInfo = QueryMethod.MakeGenericMethod(typeof(TQuery), responseType!);
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

        protected override async ValueTask<ISubscription> ProduceSubscribeQueryResponseAsync<TQuery, TQueryResponse>(Func<IReceivedMessage<TQuery>, ValueTask<QueryResponseMessage<TQueryResponse>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, MessageFilters<TQuery>? messageFilter, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Producing QueryResponse Subscription for {TQuery} responding with {TQueryResponse} on {Channel} in {Group}", typeof(TQuery), typeof(TQueryResponse), channel, group);
            var queryMessageFactory = GetMessageFactory<TQuery>(ignoreMessageHeader);
            var responseMessageFactory = GetMessageFactory<TQueryResponse>();
            var connection = await GetConnectionAsync<TQuery>(channel, ChannelMapper.MapTypes.QuerySubscription);
            return await CreateSubscriptionAsync<TQuery, TQueryResponse>(queryMessageFactory, responseMessageFactory, connection.Connection.MessageServiceConnection, messageReceived, errorReceived, connection.Channel, group, synchronous, connection.Connection.ServiceConnectionName, messageFilter, cancellationToken);
        }
        #endregion
    }
}
