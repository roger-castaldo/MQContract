using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using MQContract.Middleware;
using MQContract.Subscriptions;
using System.Diagnostics;
using System.Reflection;

namespace MQContract.Connections
{
    internal class MultiServiceConnection(IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null) :
        AMappableConnection<IMultiServiceContractConnection>(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, logger, channelMapper),
        IMultiServiceContractConnection
    {
        private readonly SemaphoreSlim publishLock = new(1, 1);

        async ValueTask<IEnumerable<PingResult>> IMultiServiceContractConnection.PingAsync()
            => await FullList
                .Select(ss => ss.MessageServiceConnection)
                .OfType<IPingableMessageServiceConnection>()
                .WhenAll(pmc => pmc.PingAsync());

        protected override async ValueTask InternalDisposeAsync()
        {
            publishLock.Dispose();
            await base.InternalDisposeAsync();
        }

        IMultiServiceContractConnection IMultiServiceContractConnection.RegisterServiceConnection(string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            => RegisterServiceConnection(pars => true, serviceConnectionName, messageServiceConnection);

        #region PubSub
        private static async ValueTask<ChildTransmissionResult> AwaitTransmission(string connectionName, Func<ValueTask<TransmissionResult>> transmit)
        {
            var result = await transmit();
            return new(connectionName, result.Error);
        }

        async ValueTask<MultiTransmissionResult> IMultiServiceContractConnection.PublishAsync<T>(T message, string? channel, MessageHeader? messageHeader, CancellationToken cancellationToken)
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
            var connections = await GetConnectionsAsync(serviceMessage.Channel, typeof(T), serviceMessage.Header);
            await publishLock.WaitAsync(cancellationToken);
            var results = await connections
                .WhenAll(c => AwaitTransmission(c.ServiceConnectionName, async () =>
                {
                    OpenTelemetryMiddleware.AssignConnectionType(activity, c.MessageServiceConnection, c.ServiceConnectionName);
                    var result = await ExecuteResilliantTransmissionAsync<T>(
                        (ct) => c.MessageServiceConnection.PublishAsync(
                            serviceMessage,
                            ct
                        ),
                        c.ServiceConnectionName,
                        serviceMessage.Channel,
                        cancellationToken
                    );
                    OpenTelemetryMiddleware.AddMessagePublishedEvent(activity, serviceMessage, result, c.MessageServiceConnection, c.ServiceConnectionName);
                    return result;
                }));
            publishLock.Release();
            activity?.SetStatus(results.Any(r => r.IsError) ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            activity?.Stop();
            return new(serviceMessage.ID, results);
        }

        async ValueTask<IEnumerable<MultiTransmissionResult>> IMultiServiceContractConnection.BulkPublishAsync<T>(IEnumerable<(T message, MessageHeader? messageHeader)> messages, string? channel, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Bulk Publishing messages {T} on {Channel}", typeof(T), channel);
            using var activity = StartActivity(Constants.BulkPublishActivityName);
            activity?.SetTag(Constants.BulkPublishCountTag, messages.Count());
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
            var connections = await GetConnectionsAsync(serviceMessages.First().Channel, typeof(T), serviceMessages.First().Header);
            await publishLock.WaitAsync(cancellationToken);
            var transmissionResults = await Task.WhenAll(connections.Select(c => Task<MultiTransmissionResult>.Run(async () =>
            {
                OpenTelemetryMiddleware.AssignConnectionType(activity, c.MessageServiceConnection, c.ServiceConnectionName);
                var result = await BulkPublishAsync<T>(serviceMessages, c.MessageServiceConnection, activity, cancellationToken, connectionName: c.ServiceConnectionName);
                return result.Select((res, index) => new MultiTransmissionResult(serviceMessages.ElementAt(index).ID, [new(c.ServiceConnectionName, res.Error)]));
            })));
            publishLock.Release();
            activity?.SetStatus(Array.Exists(transmissionResults, mtr => mtr.Any(r => r.HasError)) ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            activity?.Stop();
            return transmissionResults
                .SelectMany(mtr => mtr)
                .GroupBy(mtr => mtr.ID)
                .Select(grp => new MultiTransmissionResult(grp.Key, grp.SelectMany(g => g.Results)));
        }

        protected override async ValueTask<ISubscription> CreateSubscriptionAsync<T>(Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<T>? messageFilters, bool synchronous, CancellationToken cancellationToken)
        {
            var messageFactory = GetMessageFactory<T>(ignoreMessageHeader);
            (var connections, channel) = await GetConnectionsAsync<T>(channel, ChannelMapper.MapTypes.PublishSubscription);
            return new SubscriptionCollection(await connections.WhenAll(conn =>
                CreateSubscriptionAsync<T>(
                    messageFactory,
                    conn.MessageServiceConnection,
                    messageReceived,
                    errorReceived,
                    channel,
                    group,
                    synchronous,
                    conn.ServiceConnectionName,
                    messageFilters,
                    cancellationToken
                ))
            );
        }
        #endregion

        #region QueryResponse
        async ValueTask<IEnumerable<QueryResult<R>>> IMultiServiceContractConnection.QueryAsync<Q, R>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
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
            var connections = await GetConnectionsAsync(serviceMessage.Channel, typeof(Q), serviceMessage.Header);
            return await connections
                .WhenAll(conn =>
                {
                    OpenTelemetryMiddleware.AssignConnectionType(activity, conn.MessageServiceConnection, conn.ServiceConnectionName);
                    return ExecuteQueryAsync<Q, R>(conn.MessageServiceConnection, serviceMessage, activity, timeout: timeout, responseChannel: responseChannel, connectionName: conn.ServiceConnectionName, cancellationToken: cancellationToken);
                });
        }

        private static readonly MethodInfo QueryMethod = typeof(IMultiServiceContractConnection).GetMethods()
            .First(method => Equals(method.Name, nameof(IMultiServiceContractConnection.QueryAsync)) && method.GetGenericArguments().Length==2);
        async ValueTask<IEnumerable<QueryResult<object>>> IMultiServiceContractConnection.QueryAsync<Q>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Attempting to get response type for QueryResponse for {Q} on {Channel} with {ResponseChannel}", typeof(Q), channel, responseChannel);
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var responseType = (typeof(Q).GetCustomAttribute<QueryResponseTypeAttribute>(false)?.ResponseType)??throw new UnknownResponseTypeException("ResponseType", typeof(Q));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            Logger?.LogInformation("Obtained {ResponseType} for QueryResponse for {Q} on {Channel} with {ResponseChannel}", responseType, typeof(Q), channel, responseChannel);
            var methodInfo = QueryMethod.MakeGenericMethod(typeof(Q), responseType!);
            IEnumerable<object> results;
            try
            {
                results = (IEnumerable<object>)(await Utility.InvokeMethodAsync(
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
            return results.Select(o => Utility.ConvertResultFromObject(o)!);
        }

        protected override async ValueTask<ISubscription> ProduceSubscribeQueryResponseAsync<Q, R>(Func<IReceivedMessage<Q>, ValueTask<QueryResponseMessage<R>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Producing QueryResponse Subscription for {Q} responding with {R} on {Channel} in {Group}", typeof(Q), typeof(R), channel, group);
            var queryMessageFactory = GetMessageFactory<Q>(ignoreMessageHeader);
            var responseMessageFactory = GetMessageFactory<R>();
            (var connections, channel) = await GetConnectionsAsync<Q>(channel, ChannelMapper.MapTypes.QuerySubscription);
            return new SubscriptionCollection(await connections
                .WhenAll(conn =>
                    CreateSubscriptionAsync<Q, R>(
                       queryMessageFactory,
                       responseMessageFactory,
                       conn.MessageServiceConnection,
                       messageReceived,
                       errorReceived,
                       channel,
                       group,
                       synchronous,
                       conn.ServiceConnectionName,
                       cancellationToken
                    )
               )
            );
        }
        #endregion
    }
}
