using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Factories;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Factories;
using MQContract.Interfaces.Middleware;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using MQContract.Middleware;
using MQContract.Subscriptions;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace MQContract.Connections
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
    internal abstract partial class AConnection<CC>(IMessageEncoder? defaultMessageEncoder = null,
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null)
        : IMetricContractConnection<CC>
        where CC : IBaseContractConnection
    {
        private bool disposedValue;
        protected readonly Guid indentifier = Guid.NewGuid();
        protected readonly SemaphoreSlim dataLock = new(1, 1);
        private readonly MiddlewareCollection middleware = new(logger,channelMapper, defaultMessageEncryptor, serviceProvider);
        private readonly SemaphoreSlim inboxSemaphore = new(1, 1);
        private readonly Dictionary<Guid, TaskCompletionSource<ServiceQueryResult>> inboxResponses = [];
        private readonly Dictionary<string, IServiceSubscription> inboxSubscriptions = [];
        private IEnumerable<IMessageTypeFactory> typeFactories = [];
        private readonly List<ISubscription> consumerSubscriptions = [];
        protected ILogger? Logger => logger;
        protected IDisposable? SetScope(string? messageID = null) => logger?.BeginScope<string>($"Connection[{indentifier}]{(messageID==null ? "" : $"|Message[{messageID}]")}");

        protected IMessageFactory<T> GetMessageFactory<T>(bool ignoreMessageHeader = false)
        {
            using var scope = SetScope();
            logger?.LogInformation("Obtaining message factory for {Type}", typeof(T));
            dataLock.Wait();
            var result = (IMessageFactory<T>?)typeFactories.FirstOrDefault(fact => fact.GetType().GetGenericArguments()[0] == typeof(T));
            dataLock.Release();
            if (result == null)
            {
                logger?.LogInformation("Cached message factory for {Type} was not found, establishing a new instance and caching it", typeof(T));
                result = new MessageTypeFactory<T>(defaultMessageEncoder, serviceProvider, ignoreMessageHeader);
                dataLock.Wait();
                if (!typeFactories.Any(fact => fact.GetType().GetGenericArguments()[0] == typeof(T) && fact.IgnoreMessageHeader == ignoreMessageHeader))
                    typeFactories = typeFactories.Concat([result]);
                dataLock.Release();
            }
            return result;
        }

        protected ValueTask<string> MapChannel(ChannelMapper.MapTypes mapType, string originalChannel)
            => channelMapper?.MapChannel(mapType, originalChannel) ?? ValueTask.FromResult(originalChannel);

        #region Middleware

        private CC RegisterMiddlewareInstance(object element)
        {
            using var scope = SetScope();
            middleware.RegisterMiddlewareInstance(element);
            return (CC)(IBaseContractConnection)this;
        }

        private CC RegisterMiddlewareType(Type type)
            => RegisterMiddlewareInstance((serviceProvider == null ? Activator.CreateInstance(type) : ActivatorUtilities.CreateInstance(serviceProvider, type))!);

        CC IMiddlewareContractConnection<CC>.RegisterMiddleware<T>()
            => RegisterMiddlewareType(typeof(T));

        CC IMiddlewareContractConnection<CC>.RegisterMiddleware(Type middleware)
            => RegisterMiddlewareType(middleware);

        CC IMiddlewareContractConnection<CC>.RegisterMiddleware(IMiddleware instance)
            => RegisterMiddlewareInstance(instance);

        CC IMiddlewareContractConnection<CC>.RegisterMiddleware<T>(Func<T> constructInstance)
            => RegisterMiddlewareInstance(constructInstance());

        CC IMiddlewareContractConnection<CC>.RegisterMiddleware(Func<IMiddleware> constructInstance)
            => RegisterMiddlewareInstance(constructInstance());

        CC IMiddlewareContractConnection<CC>.RegisterMiddleware<M>(Func<ISpecificTypeMiddleware<M>> constructInstance)
            => RegisterMiddlewareInstance(constructInstance());

        CC IMiddlewareContractConnection<CC>.RegisterMiddleware<M>(ISpecificTypeMiddleware<M> instance)
            => RegisterMiddlewareInstance(instance);

        CC IMiddlewareContractConnection<CC>.RegisterMiddleware<T, M>()
            => RegisterMiddlewareType(typeof(T));

        CC IMiddlewareContractConnection<CC>.RegisterMiddleware<T, M>(Func<T> constructInstance)
            => RegisterMiddlewareInstance(constructInstance());

        private async ValueTask<(T message, string? channel, MessageHeader messageHeader)> BeforeMessageEncodeAsync<T>(IContext context, T message, string? channel, MessageHeader messageHeader)
        {
            using var scope = SetScope();
            var (genericHandlers, specificHandlers) = middleware.GetHandlers<IBeforeEncodeMiddleware,IBeforeEncodeSpecificTypeMiddleware<T>>();
            logger?.LogDebug("Executing generic Before Message Encode middleware for message of type {Type}", typeof(T));
            foreach (var handler in genericHandlers)
                (message, channel, messageHeader) = await handler.BeforeMessageEncodeAsync<T>(context, message, channel, messageHeader);
            logger?.LogDebug("Executing specific for type Before Message Encode middleware for message of type {Type}", typeof(T));
            foreach (var handler in specificHandlers)
                (message, channel, messageHeader) = await handler.BeforeMessageEncodeAsync(context, message, channel, messageHeader);
            return (message, channel, messageHeader);
        }

        private async ValueTask<ServiceMessage> AfterMessageEncodeAsync<T>(IContext context, ServiceMessage message)
        {
            using var scope = SetScope(message.ID);
            logger?.LogDebug("Executing After Message Encode middleware for message of type {Type}", typeof(T));
            var genericHandlers = middleware.GetHandlers<IAfterEncodeMiddleware>();
            logger?.LogDebug("Executing generic After Message Encode middleware for message of type {Type}", typeof(T));
            foreach (var handler in genericHandlers)
                message = await handler.AfterMessageEncodeAsync(typeof(T), context, message);
            return message;
        }

        private async ValueTask<(MessageHeader messageHeader, ReadOnlyMemory<byte> data)> BeforeMessageDecodeAsync(IContext context, string id, MessageHeader messageHeader, string messageTypeID, string messageChannel, ReadOnlyMemory<byte> data)
        {
            using var scope = SetScope(id);
            logger?.LogDebug("Executing Before Message Decode middleware");
            var genericHandlers = middleware.GetHandlers<IBeforeDecodeMiddleware>();
            logger?.LogDebug("Executing generic Before Message Decode middleware");
            foreach (var handler in genericHandlers)
                (messageHeader, data) = await handler.BeforeMessageDecodeAsync(context, id, messageHeader, messageTypeID, messageChannel, data);
            return (messageHeader, data);
        }

        private async ValueTask<(T message, MessageHeader messageHeader)> AfterMessageDecodeAsync<T>(IContext context, T message, string ID, MessageHeader messageHeader, DateTime receivedTimestamp, DateTime processedTimeStamp)
        {
            using var scope = SetScope(ID);
            var (genericHandlers, specificHandlers) = middleware.GetHandlers<IAfterDecodeMiddleware,IAfterDecodeSpecificTypeMiddleware<T>>();
            logger?.LogDebug("Executing generic After Message Decode middleware for message of type {Type}", typeof(T));
            foreach (var handler in genericHandlers)
                (message, messageHeader) = await handler.AfterMessageDecodeAsync<T>(context, message, ID, messageHeader, receivedTimestamp, processedTimeStamp);
            logger?.LogDebug("Executing specific for type After Message Decode middleware for message of type {Type}", typeof(T));
            foreach (var handler in specificHandlers)
                (message, messageHeader) = await handler.AfterMessageDecodeAsync(context, message, ID, messageHeader, receivedTimestamp, processedTimeStamp);
            return (message, messageHeader);
        }

        protected async ValueTask<ServiceMessage> ProduceServiceMessageAsync<T>(ChannelMapper.MapTypes mapType, IMessageFactory<T> messageFactory, T message, bool ignoreChannel, Activity? activity, uint? maxMessageSize = null, string? channel = null, MessageHeader? messageHeader = null)
        {
            using var scope = SetScope();
            logger?.LogDebug("Producing Service Message for message of type {Type}", typeof(T));
            var context = new Middleware.Context(mapType, activity, maxMessageSize);
            (message, channel, messageHeader) = await BeforeMessageEncodeAsync<T>(context, message, channel??messageFactory.MessageChannel, messageHeader??new([]));
            return await AfterMessageEncodeAsync<T>(context,
                await messageFactory.ConvertMessageAsync(message, ignoreChannel, channel, messageHeader)
            );
        }

        protected async ValueTask<DecodeServiceMessageResult<T>> DecodeServiceMessageAsync<T>(ChannelMapper.MapTypes mapType, IMessageFactory<T> messageFactory, ReceivedServiceMessage message, Activity? activity,MessageFilters<T>? messageFilters)
        {
            using var scope = SetScope(message.ID);
            logger?.LogDebug("Filtering Service Message message of type {Type} by headers", typeof(T));
            var filterResult = (messageFilters!=null && messageFilters.HeaderFilter!=null ? await messageFilters.HeaderFilter(message.Header) : MessageFilterResult.Allow);
            if (filterResult != MessageFilterResult.Allow)
            {
                activity?.AddEvent(new(Constants.MessageFilteredName, tags: new([
                    new($"{OpenTelemetryMiddleware.KeyBase}.filterresult",filterResult),
                    new($"{OpenTelemetryMiddleware.KeyBase}.filtertype","header")
                ])));
                return DecodeServiceMessageResult<T>.ProduceResult(filterResult);
            }
            logger?.LogDebug("Decoding Service Message message of type {Type}", typeof(T));
            var context = new Middleware.Context(mapType, activity, expectedType:typeof(T));
            (var messageHeader, var data) = await BeforeMessageDecodeAsync(context, message.ID, message.Header, message.MessageTypeID, message.Channel, message.Data);
            var taskMessage = await messageFactory.ConvertMessageAsync(logger, new ReceivedServiceMessage(message.ID, message.MessageTypeID, message.Channel, messageHeader, data, message.Acknowledge))
                                ??throw new InvalidCastException($"Unable to convert incoming message {message.MessageTypeID} to {typeof(T).FullName}");
            filterResult = (messageFilters!=null && messageFilters.MessageFilter!=null ? await messageFilters.MessageFilter(taskMessage, messageHeader) : MessageFilterResult.Allow);
            if (filterResult != MessageFilterResult.Allow)
            {
                context.Activity?.AddEvent(new(Constants.MessageFilteredName, tags: new([
                    new($"{OpenTelemetryMiddleware.KeyBase}.filterresult",filterResult),
                    new($"{OpenTelemetryMiddleware.KeyBase}.filtertype","message")
                ])));
                return DecodeServiceMessageResult<T>.ProduceResult(filterResult);
            }
            (var messageResult,var headerResult)= await AfterMessageDecodeAsync<T>(context, taskMessage!, message.ID, messageHeader, message.ReceivedTimestamp, DateTime.Now);
            return DecodeServiceMessageResult<T>.ProduceResult(messageResult, headerResult);
        }
        #endregion

        #region OTEL
        private OpenTelemetryMiddleware? openTelemetryMiddleware;

        CC IMetricContractConnection<CC>.EnableOpenTelemetry(string activitySource, bool linkActivitiesAcrossSystems)
        {
            openTelemetryMiddleware = new(activitySource, linkActivitiesAcrossSystems);
            middleware.RegisterInjectionMiddleware<IBeforeEncodeMiddleware>(openTelemetryMiddleware, MiddlewareCollection.InjectionPositions.Pre);
            middleware.RegisterInjectionMiddleware<IAfterEncodeMiddleware>(openTelemetryMiddleware, MiddlewareCollection.InjectionPositions.Post);
            middleware.RegisterInjectionMiddleware<IBeforeDecodeMiddleware>(openTelemetryMiddleware, MiddlewareCollection.InjectionPositions.Pre);
            middleware.RegisterInjectionMiddleware<IAfterDecodeMiddleware>(openTelemetryMiddleware, MiddlewareCollection.InjectionPositions.Post);
            return (CC)(IBaseContractConnection)this;
        }


        protected Activity? StartActivity(string name, MessageHeader? messageHeader = null, IMessageServiceConnection? serviceConnection = null, string? connectionName = null, Activity? current = null)
            => openTelemetryMiddleware?.StartActivity(name, messageHeader, serviceConnection, connectionName, current);
        #endregion

        #region Metrics

        private MetricsMiddleware? metricsMiddleware;

        CC IMetricContractConnection<CC>.AddMetrics(Meter? meter, bool useInternal)
        {
            using var scope = SetScope();
            logger?.LogDebug("Enabling metrics on service connection with {Meter} and {UseInternal}", meter, useInternal);
            metricsMiddleware = new MetricsMiddleware(meter, useInternal);
            middleware.RegisterInjectionMiddleware<IBeforeEncodeMiddleware>(metricsMiddleware, MiddlewareCollection.InjectionPositions.Pre);
            middleware.RegisterInjectionMiddleware<IAfterEncodeMiddleware>(metricsMiddleware, MiddlewareCollection.InjectionPositions.Post);
            middleware.RegisterInjectionMiddleware<IBeforeDecodeMiddleware>(metricsMiddleware, MiddlewareCollection.InjectionPositions.Pre);
            middleware.RegisterInjectionMiddleware<IAfterDecodeMiddleware>(metricsMiddleware, MiddlewareCollection.InjectionPositions.Post);
            return (CC)(IBaseContractConnection)this;
        }

        IContractMetric? IMetricContractConnection<CC>.GetSnapshot(bool sent)
            => metricsMiddleware?.GetSnapshot(sent);
        IContractMetric? IMetricContractConnection<CC>.GetSnapshot(Type messageType, bool sent)
            => metricsMiddleware?.GetSnapshot(messageType, sent);
        IContractMetric? IMetricContractConnection<CC>.GetSnapshot<T>(bool sent)
            => metricsMiddleware?.GetSnapshot(typeof(T), sent);
        IContractMetric? IMetricContractConnection<CC>.GetSnapshot(string channel, bool sent)
            => metricsMiddleware?.GetSnapshot(channel, sent);
        #endregion

        #region Subscriptions
        protected abstract ValueTask<ISubscription> CreateSubscriptionAsync<TMessage>(Func<IReceivedMessage<TMessage>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, bool synchronous, CancellationToken cancellationToken);

        ValueTask<ISubscription> IBaseContractConnection.SubscribeAsync<TMessage>(Func<IReceivedMessage<TMessage>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            logger?.LogDebug("Creating PubSub subscription for message type {T} on channel {Channel} in group {Group}", typeof(TMessage), channel, group);
            return CreateSubscriptionAsync<TMessage>(messageReceived, errorReceived, channel, group, ignoreMessageHeader, messageFilters, false, cancellationToken);
        }

        ValueTask<ISubscription> IBaseContractConnection.SubscribeAsync<TMessage>(Action<IReceivedMessage<TMessage>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            logger?.LogDebug("Creating PubSub subscription for message type {T} on channel {Channel} in group {Group}", typeof(TMessage), channel, group);
            return CreateSubscriptionAsync<TMessage>((msg) =>
            {
                messageReceived(msg);
                return ValueTask.CompletedTask;
            },
            errorReceived, channel, group, ignoreMessageHeader, messageFilters, true, cancellationToken);
        }
        protected abstract ValueTask<ISubscription> ProduceSubscribeQueryResponseAsync<TQuery, TQueryResponse>(Func<IReceivedMessage<TQuery>, ValueTask<QueryResponseMessage<TQueryResponse>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, MessageFilters<TQuery>? messageFilter, CancellationToken cancellationToken);

        ValueTask<ISubscription> IBaseContractConnection.SubscribeQueryAsyncResponseAsync<TQuery, TQueryResponse>(Func<IReceivedMessage<TQuery>, ValueTask<QueryResponseMessage<TQueryResponse>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TQuery>? messageFilters, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            logger?.LogDebug("Creating QueryResponse subscription for message query {Q} and response {R} on channel {Channel} in group {Group}", typeof(TQuery), typeof(TQueryResponse), channel, group);
            return ProduceSubscribeQueryResponseAsync<TQuery, TQueryResponse>(messageReceived, errorReceived, channel, group, ignoreMessageHeader, false, messageFilters, cancellationToken);
        }

        ValueTask<ISubscription> IBaseContractConnection.SubscribeQueryResponseAsync<TQuery, TQueryResponse>(Func<IReceivedMessage<TQuery>, QueryResponseMessage<TQueryResponse>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TQuery>? messageFilters, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            logger?.LogDebug("Creating QueryResponse subscription for message query {Q} and response {R} on channel {Channel} in group {Group}", typeof(TQuery), typeof(TQueryResponse), channel, group);
            return ProduceSubscribeQueryResponseAsync<TQuery, TQueryResponse>((msg) =>
            {
                var result = messageReceived(msg);
                return ValueTask.FromResult(result);
            }, errorReceived, channel, group, ignoreMessageHeader, true, messageFilters, cancellationToken);
        }
        #endregion

        #region PubSub
        protected async ValueTask<TransmissionResult> PublishMessageAsync<T>(SemaphoreSlim publishLock, ServiceMessage serviceMessage, IMessageServiceConnection serviceConnection, Activity? activity, string? connectionName, CancellationToken cancellationToken)
        {
            await publishLock.WaitAsync(cancellationToken);
            var result = await ExecuteResilliantTransmissionAsync<T>(
                (ct) => serviceConnection.PublishAsync(
                    serviceMessage,
                    ct
                ),
                connectionName,
                serviceMessage.Channel,
                cancellationToken
            );
            OpenTelemetryMiddleware.AddMessagePublishedEvent(activity, serviceMessage, result, serviceConnection, connectionName);
            publishLock.Release();
            activity?.SetStatus(result.IsError ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            activity?.Stop();
            return result;
        }
        protected async ValueTask<IEnumerable<TransmissionResult>> BulkPublishAsync<T>(IEnumerable<ServiceMessage> serviceMessages, IMessageServiceConnection serviceConnection, Activity? activity, CancellationToken cancellationToken, string? connectionName = null)
        {
            IEnumerable<TransmissionResult> result;
            using var scope = SetScope();
            logger?.LogDebug("Executing bulk publish");
            if (serviceConnection is IBulkPublishableMessageServiceConnection bulkPublishableMessageServiceConnection)
            {
                logger?.LogInformation("Executing bulk publish against a service connection that supports bulk publish");
                result = await ExecuteResilliantTransmissionAsync<T>(
                    bulkPublishableMessageServiceConnection.BulkPublishAsync,
                    connectionName,
                    serviceMessages,
                    cancellationToken
                );
                if (activity!=null)
                {
                    foreach (var res in result)
                        activity?.AddEvent(new(Constants.PublishBulkMessagesMessageEvent, tags: new([
                            new($"{OpenTelemetryMiddleware.KeyBase}.bulksupported",true),
                            new(OpenTelemetryMiddleware.MessageIdKey,res.ID),
                            OpenTelemetryMiddleware.CreateMessagePublishStatusTag(res),
                            OpenTelemetryMiddleware.CreateConnectionTypeTag(serviceConnection)
                       ])));
                }
            }
            else
            {
                logger?.LogInformation("Executing bulk publish against a service connection that does not support bulk publish");
                result = await serviceMessages
                    .WhenAll(async message =>
                    {
                        var result = await ExecuteResilliantTransmissionAsync<T>(
                            (ct) => serviceConnection.PublishAsync(
                                message,
                                ct
                            ),
                            connectionName,
                            message.Channel,
                            cancellationToken
                        );
                        activity?.AddEvent(new(Constants.PublishBulkMessagesMessageEvent, tags: new([
                            new($"{OpenTelemetryMiddleware.KeyBase}.bulksupported",false),
                            new(OpenTelemetryMiddleware.MessageIdKey,message.ID),
                            OpenTelemetryMiddleware.CreateMessagePublishStatusTag(result),
                            OpenTelemetryMiddleware.CreateConnectionTypeTag(serviceConnection)
                        ])));
                        return result;
                    });
            }
            return result;
        }

#pragma warning disable S4136 // Method overloads should be grouped together
        protected async ValueTask<ISubscription> CreateSubscriptionAsync<T>(IMessageFactory<T> messageFactory, IMessageServiceConnection serviceConnection, Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, 
            string? channel, string? group, bool synchronous, string? serviceConnectionName, MessageFilters<T>? messageFilters, CancellationToken cancellationToken)
#pragma warning restore S4136 // Method overloads should be grouped together
        {
            using var scope = SetScope();
            logger?.LogDebug("Creating PubSub Subscription for {T} on {Channel} in {Group}.", typeof(T), channel, group);
            var subscription = new PubSubSubscription<T>(
                async (serviceMessage) =>
                {
                    using var activity = StartActivity(Constants.ConsumeActivityName, messageHeader:serviceMessage.Header, serviceConnection:serviceConnection, connectionName:serviceConnectionName);
                    try
                    {
                        var decodedResult = await DecodeServiceMessageAsync<T>(ChannelMapper.MapTypes.PublishSubscription, messageFactory, serviceMessage, activity, messageFilters);
                        if (decodedResult.FilterResult == MessageFilterResult.Allow)
                            await messageReceived(new ReceivedMessage<T>(serviceMessage.ID, decodedResult.Message!, decodedResult.Header!, serviceMessage.ReceivedTimestamp, DateTime.Now, activity));
                        activity?.SetStatus(ActivityStatusCode.Ok);
                        return !Equals(decodedResult.FilterResult, MessageFilterResult.DropAndDontAcknowledge);
                    }
                    catch
                    {
                        activity?.SetStatus(ActivityStatusCode.Error);
                        throw;
                    }
                },
                errorReceived,
                (originalChannel) => MapChannel(ChannelMapper.MapTypes.PublishSubscription, originalChannel)!,
                channel: channel,
            group: group,
            synchronous: synchronous,
                logger: Logger);
            logger?.LogInformation("Establishing PubSub Subscription connection.");
            if (await subscription.EstablishSubscriptionAsync(serviceConnection, cancellationToken))
                return subscription;
            logger?.LogInformation("Establishment of PubSub Subscription connection failed.");
            throw new SubscriptionFailedException();
        }
        #endregion

        #region QueryResponse
        private async ValueTask<(ServiceQueryResult? serviceQueryResult, ErrorMessage? errorMessage)> ProcessInboxMessageAsync<T>(string? connectionName, IInboxQueryableMessageServiceConnection inboxMessageServiceConnection, ServiceMessage serviceMessage, TimeSpan timeout, Activity? activity, CancellationToken cancellationToken)
        {
            using var scope = SetScope(serviceMessage.ID);
            logger?.LogDebug("Establishing an instance of Inbox Message style handling for a QueryResponse call on {ConnectionName}", connectionName);
            var messageID = Guid.NewGuid();
            logger?.LogInformation("Setting up Inbox Message listener with {CorrelationID}", messageID);
            await inboxSemaphore.WaitAsync(cancellationToken);
            if (!inboxSubscriptions.TryGetValue(connectionName??"DEFAULT", out var inboxSubscription))
            {
                logger?.LogDebug("Establishing new Inbox Subscription for {ConnectionName}", connectionName);
                inboxSubscription = await inboxMessageServiceConnection.EstablishInboxSubscriptionAsync(
                    async (message) =>
                    {
                        try
                        {
                            await inboxSemaphore.WaitAsync();
                        }
                        catch
                        {
                            return;
                        }
                        if (message.Acknowledge!=null)
                            await message.Acknowledge();
                        using var scope = SetScope(message.ID);
                        logger?.LogInformation("Attempting to process Inbox message with {CorrelationID}", message.CorrelationID);
                        if (inboxResponses.TryGetValue(message.CorrelationID, out var taskCompletionSource))
                        {
                            taskCompletionSource.TrySetResult(new(
                                message.ID,
                                message.Header,
                                message.MessageTypeID,
                                message.Data
                            ));
                        }
                        inboxSemaphore.Release();
                    },
                    cancellationToken
                );
                inboxSubscriptions.Add(connectionName ?? "DEFAULT", inboxSubscription);
            }
            var tcs = new TaskCompletionSource<ServiceQueryResult>();
            inboxResponses.Add(messageID, tcs);
            inboxSemaphore.Release();
            using var token = new CancellationTokenSource();
            var reg = cancellationToken.Register(() => token.Cancel());
            token.Token.Register(async () =>
            {
                await reg.DisposeAsync();
                if (!tcs.Task.IsCompleted)
                {
                    using var scope = SetScope(serviceMessage.ID);
                    logger?.LogDebug("Inbox Query Message has timed out waiting for the response");
                    tcs.TrySetException(new QueryTimeoutException());
                }
            });
            token.CancelAfter(timeout);
            logger?.LogInformation("Transmitting Inbox Query request to underlying system with {CorrelationID} and being waiting on response", messageID);
            var result = await ExecuteResilliantTransmissionAsync<T>(
                async (ct) => await inboxMessageServiceConnection.QueryAsync(serviceMessage, messageID, ct),
                connectionName,
                serviceMessage.Channel,
                cancellationToken
            );
            OpenTelemetryMiddleware.AddMessagePublishedEvent(activity, serviceMessage, result, inboxMessageServiceConnection, connectionName);
            if (result.IsError)
            {
                if (!token.IsCancellationRequested)
                    await token.CancelAsync();
                logger?.LogInformation("Inbox Query tranmission failed cleaning up resources");
                await inboxSemaphore.WaitAsync(cancellationToken);
                inboxResponses.Remove(messageID);
                inboxSemaphore.Release();
                return (null, result.Error);
            }
            try
            {
                await tcs.Task.WaitAsync(cancellationToken);
            }
            catch (TaskCanceledException tce)
            {
                return (null, new(tce, true));
            }
            finally
            {
                if (!token.IsCancellationRequested)
                    await token.CancelAsync();
                await inboxSemaphore.WaitAsync(CancellationToken.None);
                inboxResponses.Remove(messageID);
                inboxSemaphore.Release();
            }
            return (tcs.Task.Result, null);
        }
        protected async ValueTask<QueryResult<TQueryResult>> ProduceResultAsync<TQueryResult>(ServiceQueryResult queryResult, IMessageServiceConnection serviceConnection, string? serviceConnectionName, string responseChannel = "")
        {
            using var scope = SetScope(queryResult.ID);
            logger?.LogDebug("Attempting to produce a Query Result of {R} from the Service Message of the type {MessageTypeID}", typeof(TQueryResult), queryResult.MessageTypeID);
            QueryResult<TQueryResult> result;
            using var activity = StartActivity(Constants.ConsumeQueryResponseActivityName, messageHeader: queryResult.Header, serviceConnection: serviceConnection, connectionName: serviceConnectionName);
            try
            {
                var decodeResult = await DecodeServiceMessageAsync<TQueryResult>(
                    ChannelMapper.MapTypes.QueryResponse, 
                    GetMessageFactory<TQueryResult>(true), 
                    new(queryResult.ID, queryResult.MessageTypeID, responseChannel, queryResult.Header, queryResult.Data), 
                    activity,
                    null
                );
                result = new QueryResult<TQueryResult>(
                    queryResult.ID,
                    decodeResult.Header!,
                    Result: decodeResult.Message
                );
            }
            catch (QueryResponseException qre)
            {
                logger?.LogError(qre, "A query response exception occured");
                result = new(
                    queryResult.ID,
                    queryResult.Header,
                    Result: default,
                    Error: new(qre)
                );
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occured attempting to convert the Service Message of the type {MessageTypeID} to the Query Result of {R}", queryResult.MessageTypeID, typeof(TQueryResult));
                result = new(
                    queryResult.ID,
                    queryResult.Header,
                    Result: default,
                    Error: new(ex)
                );
            }
            activity?.SetStatus(result.IsError ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            activity?.Stop();
            return result;
        }

        protected async ValueTask<QueryResult<R>> ExecuteQueryAsync<Q, R>(IMessageServiceConnection serviceConnection, ServiceMessage serviceMessage, Activity? activity, TimeSpan? timeout = null, string? responseChannel = null, string? connectionName = null, CancellationToken cancellationToken = new CancellationToken())
        {
            using var scope = SetScope(serviceMessage.ID);
            logger?.LogDebug("Attempting to execute a Query of {Q} with a response {R}", typeof(Q), typeof(R));
            var realTimeout = timeout??typeof(Q).GetCustomAttribute<QueryMessageAttribute>()?.ResponseTimeout;
            activity?.SetStatus(ActivityStatusCode.Ok);
            try
            {
                if (serviceConnection is IQueryResponseMessageServiceConnection queryableMessageServiceConnection)
                {
                    logger?.LogInformation("Executing a QueryResponse call on a QueryResponse service connection");
                    return await ExecuteResilliantTransmissionAsync<Q, R>(
                        async (ct) =>
                        {
                            ServiceQueryResult result;
                            try
                            {
                                result = await queryableMessageServiceConnection.QueryAsync(
                                        serviceMessage,
                                        realTimeout??queryableMessageServiceConnection.DefaultTimeout,
                                        ct
                                );
                            }
                            catch (TransmissionException te)
                            {
                                return new QueryResult<R>(serviceMessage.ID, new([]), Error: new(te));
                            }
                            return await ProduceResultAsync<R>(
                                result,
                                serviceConnection,
                                connectionName
                            );
                        },
                        connectionName,
                        serviceMessage.Channel,
                        cancellationToken
                    );
                }
                else if (serviceConnection is IInboxQueryableMessageServiceConnection inboxMessageServiceConnection)
                {
                    logger?.LogInformation("Executing a QueryResponse call on an InboxQuery service connection");
                    var (serviceQueryResult, errorMessage)= await ProcessInboxMessageAsync<Q>(connectionName, inboxMessageServiceConnection, serviceMessage, realTimeout??inboxMessageServiceConnection.DefaultTimeout, activity, cancellationToken);
                    if (serviceQueryResult!=null)
                        return await ProduceResultAsync<R>(
                            serviceQueryResult!,
                            serviceConnection,
                            connectionName
                        );
                    activity?.SetStatus(ActivityStatusCode.Error);
                    return new(serviceMessage.ID, new([]), Error: errorMessage);
                }
                logger?.LogInformation("Executing a QueryResponse call on a standard PubSub service connection using {ResponseChannel}", responseChannel);
                return await ProcessPubSubQuery<Q, R>(serviceConnection, connectionName, responseChannel, realTimeout, serviceMessage, activity, cancellationToken);
            }
            catch (Exception ex)
            {
                if (ex is QueryTimeoutException || ex is QueryExecutionFailedException)
                    activity?.SetStatus(ActivityStatusCode.Error);
                throw;
            }
        }

        protected async ValueTask<QueryResult<R>> ProcessPubSubQuery<Q, R>(IMessageServiceConnection serviceConnection, string? connectionName, string? responseChannel, TimeSpan? realTimeout, ServiceMessage serviceMessage, Activity? activity, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            responseChannel ??=typeof(Q).GetCustomAttribute<QueryMessageAttribute>()?.ResponseChannel;
            logger?.LogInformation("Attempting a QueryResponse call using PubSub style messaging, querying {Q}, expecting a response of {R} on {ResponseChannel}", typeof(Q), typeof(R), responseChannel);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(responseChannel);
            var replyChannel = await MapChannel(ChannelMapper.MapTypes.QueryResponse, responseChannel!);
            logger?.LogDebug("QueryResponse reply channel mapped to {ReplyChannel}", replyChannel);
            var callID = Guid.NewGuid();
            logger?.LogDebug("Starting Response listener for Query over PubSub waiting on a message with {CallID}", callID);
            var (tcs, token) = await QueryResponseHelper.StartResponseListenerAsync(
                serviceConnection,
                realTimeout??TimeSpan.FromMinutes(1),
                indentifier,
                callID,
                replyChannel,
                cancellationToken
            );
            var msg = QueryResponseHelper.EncodeMessage(
                serviceMessage,
                indentifier,
                callID,
                replyChannel,
                null
            );
            logger?.LogDebug("Transmitting Query request over PubSub");
            var result = await ExecuteResilliantTransmissionAsync<Q>(
                async (ct) => await serviceConnection.PublishAsync(msg, cancellationToken: ct),
                connectionName,
                serviceMessage.Channel,
                cancellationToken
            );
            OpenTelemetryMiddleware.AddMessagePublishedEvent(activity, msg, result, serviceConnection, connectionName);
            if (result.IsError)
            {
                if (!token.IsCancellationRequested)
                    await token.CancelAsync();
                logger?.LogDebug("Inbox Query tranmission failed cleaning up resources");
                activity?.SetStatus(ActivityStatusCode.Error);
                return new(serviceMessage.ID, new([]), Error: result.Error);
            }
            try
            {
                logger?.LogDebug("Waiting on Query Response over PubSub");
                await tcs.Task.WaitAsync(cancellationToken);
                logger?.LogDebug("Query Response over PubSub recieved");
            }
            finally
            {
                if (!token.IsCancellationRequested)
                    await token.CancelAsync();
            }
            return await ProduceResultAsync<R>(tcs.Task.Result, serviceConnection, connectionName, responseChannel: responseChannel);
        }
        protected async ValueTask<ISubscription> CreateSubscriptionAsync<TQuery, TQueryResponse>(IMessageFactory<TQuery> queryMessageFactory, IMessageFactory<TQueryResponse> responseMessageFactory, IMessageServiceConnection serviceConnection,
            Func<IReceivedMessage<TQuery>, ValueTask<QueryResponseMessage<TQueryResponse>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool synchronous, string? serviceConnectionName, MessageFilters<TQuery>? messageFilters, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            logger?.LogInformation("Creating QueryResponse subscription for {Q} answering with {R} on {Channel} in {Group}", typeof(TQuery), typeof(TQueryResponse), channel, group);
            var subscription = new QueryResponseSubscription<TQuery>(
                async (message, replyChannel) =>
                {
                    using var consumeActivity = StartActivity(Constants.ConsumeQueryActivityName, messageHeader: message.Header, serviceConnection: serviceConnection, connectionName: serviceConnectionName);
                    DecodeServiceMessageResult<TQuery> decodedResult;
                    try
                    {
                        decodedResult = await DecodeServiceMessageAsync<TQuery>(
                            ChannelMapper.MapTypes.QuerySubscription,
                            queryMessageFactory,
                            message,
                            consumeActivity,
                            messageFilters
                        );
                    }
                    catch
                    {
                        consumeActivity?.SetStatus(ActivityStatusCode.Error);
                        throw;
                    }
                    consumeActivity?.SetStatus(ActivityStatusCode.Ok);
                    if (Equals(decodedResult.FilterResult, MessageFilterResult.Allow))
                    {
                        var result = await messageReceived(new ReceivedMessage<TQuery>(message.ID, decodedResult.Message!, decodedResult.Header!, message.ReceivedTimestamp, DateTime.Now, consumeActivity));
                        using var responseActivity = StartActivity(
                            Constants.ProduceQueryResponseActivityName,
                            serviceConnection: serviceConnection,
                            connectionName: serviceConnectionName,
                            current: consumeActivity
                        );
                        try
                        {
                            var response = await ProduceServiceMessageAsync<TQueryResponse>(
                                ChannelMapper.MapTypes.QueryResponse,
                                responseMessageFactory,
                                result.Message,
                                true,
                                responseActivity,
                                maxMessageSize: serviceConnection.MaxMessageBodySize,
                                channel: replyChannel,
                                messageHeader: new(result.Headers)
                            );
                            responseActivity?.SetStatus(ActivityStatusCode.Ok);
                            return (response, responseActivity, decodedResult.FilterResult);
                        }
                        catch
                        {
                            responseActivity?.SetStatus(ActivityStatusCode.Error);
                            throw;
                        }
                    }
                    return (null, consumeActivity, decodedResult.FilterResult);
                },
                errorReceived,
                (originalChannel) => MapChannel(ChannelMapper.MapTypes.QuerySubscription, originalChannel),
                channel: channel,
            group: group,
            synchronous: synchronous,
                logger: Logger);
            logger?.LogDebug("Establishing QueryResponse subscription");
            if (await subscription.EstablishSubscriptionAsync(serviceConnection, serviceConnectionName, cancellationToken))
                return subscription;
            logger?.LogDebug("Failed to establish subscription");
            throw new SubscriptionFailedException();
        }
        #endregion

        protected abstract ValueTask CloseAsync();
        async ValueTask IBaseContractConnection.CloseAsync()
        {
            using var scope = SetScope();
            logger?.LogDebug("Closing contract connection");
            await inboxSemaphore.WaitAsync();
            logger?.LogInformation("Closing all open inbox subscriptions");
            foreach (var key in inboxSubscriptions.Keys)
            {
                var inboxSubscription = inboxSubscriptions[key];
                await inboxSubscription.EndAsync();
            }
            foreach (var consumerSubscription in consumerSubscriptions)
                await consumerSubscription.EndAsync();
            consumerSubscriptions.Clear();
            inboxSemaphore.Release();
            await CloseAsync();
        }
        protected abstract ValueTask InternalDisposeAsync();

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                await inboxSemaphore.WaitAsync();
                foreach (var key in inboxSubscriptions.Keys)
                {
                    var inboxSubscription = inboxSubscriptions[key];
                    if (inboxSubscription is IAsyncDisposable asyncSubDisposable)
                        await asyncSubDisposable.DisposeAsync();
                    else if (inboxSubscription is IDisposable subDisposable)
                        subDisposable.Dispose();
                }
                foreach (var consumerSubscription in consumerSubscriptions)
                    await consumerSubscription.EndAsync();
                consumerSubscriptions.Clear();
                inboxSubscriptions.Clear();
                inboxSemaphore.Dispose();
                await InternalDisposeAsync();
            }
            GC.SuppressFinalize(this);
        }
    }
}
