using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQContract.Extensions;
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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MQContract.Connections
{
    internal abstract partial class AConnection<TContractConnection> : IMetricContractConnection<TContractConnection>
        where TContractConnection : IBaseContractConnection
    {
        private bool disposedValue;
        protected readonly Guid indentifier = Guid.NewGuid();
        protected readonly MessageContext messageContext = new();
        private readonly MiddlewareCollection middleware;
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<ServiceQueryResult>> inboxResponses = [];
        private readonly ConcurrentDictionary<string, IServiceSubscription> inboxSubscriptions = [];
        private readonly ConcurrentDictionary<(Type messageType, bool ignoreMessageHeader), IMessageTypeFactory> typeFactories = [];
        private readonly ConcurrentBag<ISubscription> consumerSubscriptions = [];
        protected readonly ILogger? Logger;
        private readonly IMessageEncoder? defaultMessageEncoder;
        private readonly IServiceProvider? serviceProvider;
        private readonly ChannelMapper? channelMapper;

        protected AConnection(IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null)
        {
            this.defaultMessageEncoder = defaultMessageEncoder;
            this.serviceProvider = serviceProvider;
            this.channelMapper = channelMapper;
            middleware = new(logger, channelMapper, messageContext, defaultMessageEncryptor, serviceProvider);
            Logger = logger;
        }

        protected IDisposable? SetScope(string? messageID = null) => Logger?.BeginScope<string>($"Connection[{indentifier}]{(messageID==null ? "" : $"|Message[{messageID}]")}");

        protected IMessageFactory<TMessage> GetMessageFactory<TMessage>(bool ignoreMessageHeader = false)
        {
            if (!typeFactories.TryGetValue((typeof(TMessage), ignoreMessageHeader), out var result))
            {
                result = new MessageTypeFactory<TMessage>(defaultMessageEncoder, serviceProvider, ignoreMessageHeader, messageContext);
                typeFactories.TryAdd((typeof(TMessage), ignoreMessageHeader), result);
            }
            return (IMessageFactory<TMessage>)result;
        }

        protected ValueTask<string> MapChannel(ChannelMapper.MapTypes mapType, string originalChannel)
            => channelMapper?.MapChannel(mapType, originalChannel) ?? ValueTask.FromResult(originalChannel);

        #region Middleware

        private async ValueTask<TContractConnection> RegisterMiddlewareInstanceAsync(object element)
        {
            using var scope = SetScope();
            await middleware.RegisterMiddlewareInstanceAsync(element, messageContext.Contexts);
            return (TContractConnection)(IBaseContractConnection)this;
        }

        private ValueTask<TContractConnection> RegisterMiddlewareTypeAsync(Type type)
            => RegisterMiddlewareInstanceAsync((serviceProvider == null ? Activator.CreateInstance(type) : ActivatorUtilities.CreateInstance(serviceProvider, type))!);

        ValueTask<TContractConnection> IMiddlewareContractConnection<TContractConnection>.RegisterMiddlewareAsync<TMiddleware>()
            => RegisterMiddlewareTypeAsync(typeof(TMiddleware));

        ValueTask<TContractConnection> IMiddlewareContractConnection<TContractConnection>.RegisterMiddlewareAsync(Type middleware)
            => RegisterMiddlewareTypeAsync(middleware);

        ValueTask<TContractConnection> IMiddlewareContractConnection<TContractConnection>.RegisterMiddlewareAsync(IMiddleware instance)
            => RegisterMiddlewareInstanceAsync(instance);

        ValueTask<TContractConnection> IMiddlewareContractConnection<TContractConnection>.RegisterMiddlewareAsync<TMiddleware>(Func<TMiddleware> constructInstance)
            => RegisterMiddlewareInstanceAsync(constructInstance());

        ValueTask<TContractConnection> IMiddlewareContractConnection<TContractConnection>.RegisterMiddlewareAsync(Func<IMiddleware> constructInstance)
            => RegisterMiddlewareInstanceAsync(constructInstance());

        ValueTask<TContractConnection> IMiddlewareContractConnection<TContractConnection>.RegisterMiddlewareAsync<TMessage>(Func<ISpecificTypeMiddleware<TMessage>> constructInstance)
            => RegisterMiddlewareInstanceAsync(constructInstance());

        ValueTask<TContractConnection> IMiddlewareContractConnection<TContractConnection>.RegisterMiddlewareAsync<TMessage>(ISpecificTypeMiddleware<TMessage> instance)
            => RegisterMiddlewareInstanceAsync(instance);

        ValueTask<TContractConnection> IMiddlewareContractConnection<TContractConnection>.RegisterMiddlewareAsync<TMiddleware, TMessage>()
            => RegisterMiddlewareTypeAsync(typeof(TMiddleware));

        ValueTask<TContractConnection> IMiddlewareContractConnection<TContractConnection>.RegisterMiddlewareAsync<TMiddleware, TMessage>(Func<TMiddleware> constructInstance)
            => RegisterMiddlewareInstanceAsync(constructInstance());

        private async ValueTask<EncodableMessage<TMessage>> BeforeMessageEncodeAsync<TMessage>(IContext context, TMessage message, string? channel, MessageHeader messageHeader)
        {
            using var scope = SetScope();
            var (genericHandlers, specificHandlers) = middleware.GetHandlers<IBeforeEncodeMiddleware, IBeforeEncodeSpecificTypeMiddleware<TMessage>>();
            var result = new EncodableMessage<TMessage>(messageHeader, message, channel);
            Logger?.LogDebugChecked("Executing generic Before Message Encode middleware for message of type {Type}", typeof(TMessage));
            foreach (var handler in genericHandlers)
                result = await handler.BeforeMessageEncodeAsync<TMessage>(context, result);
            Logger?.LogDebugChecked("Executing specific for type Before Message Encode middleware for message of type {Type}", typeof(TMessage));
            foreach (var handler in specificHandlers)
                result = await handler.BeforeMessageEncodeAsync(context, result);
            return result;
        }

        private async ValueTask<ServiceMessage> AfterMessageEncodeAsync<TMessage>(IContext context, ServiceMessage message)
        {
            using var scope = SetScope(message.ID);
            var genericHandlers = middleware.GetHandlers<IAfterEncodeMiddleware>();
            Logger?.LogDebugChecked("Executing generic After Message Encode middleware for message of type {Type}", typeof(TMessage));
            foreach (var handler in genericHandlers)
                message = await handler.AfterMessageEncodeAsync(typeof(TMessage), context, message);
            return message;
        }

        private async ValueTask<DecodableMessage> BeforeMessageDecodeAsync(IContext context, string id, MessageHeader messageHeader, string messageTypeID, string messageChannel, ReadOnlyMemory<byte> data)
        {
            using var scope = SetScope(id);
            var genericHandlers = middleware.GetHandlers<IBeforeDecodeMiddleware>();
            var result = new DecodableMessage(messageHeader, data);
            Logger?.LogDebugChecked("Executing generic Before Message Decode middleware");
            foreach (var handler in genericHandlers)
                result = await handler.BeforeMessageDecodeAsync(context, id, messageTypeID, messageChannel, result);
            return result;
        }

        private async ValueTask<DecodedMessage<TMessage>> AfterMessageDecodeAsync<TMessage>(IContext context, TMessage message, string ID, MessageHeader messageHeader, DateTime receivedTimestamp, DateTime processedTimeStamp)
        {
            using var scope = SetScope(ID);
            var (genericHandlers, specificHandlers) = middleware.GetHandlers<IAfterDecodeMiddleware, IAfterDecodeSpecificTypeMiddleware<TMessage>>();
            var result = new DecodedMessage<TMessage>(messageHeader, message);
            Logger?.LogDebugChecked("Executing generic After Message Decode middleware for message of type {Type}", typeof(TMessage));
            foreach (var handler in genericHandlers)
                result = await handler.AfterMessageDecodeAsync<TMessage>(context, ID, result, receivedTimestamp, processedTimeStamp);
            Logger?.LogDebugChecked("Executing specific for type After Message Decode middleware for message of type {Type}", typeof(TMessage));
            foreach (var handler in specificHandlers)
                result = await handler.AfterMessageDecodeAsync(context, ID, result, receivedTimestamp, processedTimeStamp);
            return result;
        }

        protected async ValueTask<ServiceMessage> ProduceServiceMessageAsync<TMessage>(ChannelMapper.MapTypes mapType, IMessageFactory<TMessage> messageFactory, TMessage message, bool ignoreChannel, Activity? activity, uint? maxMessageSize = null, string? channel = null, MessageHeader? messageHeader = null)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Producing Service Message for message of type {Type}", typeof(TMessage));
            var context = new Middleware.Context(mapType, activity, maxMessageSize);
            var encodableMessage = await BeforeMessageEncodeAsync<TMessage>(context, message, channel??messageFactory.MessageChannel, messageHeader??new([]));
            return await AfterMessageEncodeAsync<TMessage>(context,
                await messageFactory.ConvertMessageAsync(encodableMessage.Message, ignoreChannel, encodableMessage.Channel, encodableMessage.MessageHeader)
            );
        }

        protected async ValueTask<DecodeServiceMessageResult<TMessage>> DecodeServiceMessageAsync<TMessage>(ChannelMapper.MapTypes mapType, IMessageFactory<TMessage> messageFactory, ReceivedServiceMessage message, Activity? activity, MessageFilters<TMessage>? messageFilters)
        {
            using var scope = SetScope(message.ID);
            Logger?.LogDebugChecked("Filtering Service Message message of type {Type} by headers", typeof(TMessage));
            var filterResult = (messageFilters!=null && messageFilters.HeaderFilter!=null ? await messageFilters.HeaderFilter(message.Header) : MessageFilterResult.Allow);
            if (filterResult != MessageFilterResult.Allow)
            {
                activity?.AddEvent(new(Constants.MessageFilteredName, tags: new([
                    new($"{OpenTelemetryMiddleware.KeyBase}.filterresult",filterResult),
                    new($"{OpenTelemetryMiddleware.KeyBase}.filtertype","header")
                ])));
                return DecodeServiceMessageResult<TMessage>.ProduceResult(filterResult);
            }
            Logger?.LogDebugChecked("Decoding Service Message message of type {Type}", typeof(TMessage));
            var context = new Middleware.Context(mapType, activity, expectedType: typeof(TMessage));
            var decodableMessage = await BeforeMessageDecodeAsync(context, message.ID, message.Header, message.MessageTypeID, message.Channel, message.Data);
            var taskMessage = await messageFactory.ConvertMessageAsync(Logger, new ReceivedServiceMessage(message.ID, message.MessageTypeID, message.Channel, decodableMessage.MessageHeader, decodableMessage.Data, message.Acknowledge))
                                ??throw new InvalidCastException($"Unable to convert incoming message {message.MessageTypeID} to {typeof(TMessage).FullName}");
            filterResult = (messageFilters!=null && messageFilters.MessageFilter!=null ? await messageFilters.MessageFilter(taskMessage, decodableMessage.MessageHeader) : MessageFilterResult.Allow);
            if (filterResult != MessageFilterResult.Allow)
            {
                context.Activity?.AddEvent(new(Constants.MessageFilteredName, tags: new([
                    new($"{OpenTelemetryMiddleware.KeyBase}.filterresult",filterResult),
                    new($"{OpenTelemetryMiddleware.KeyBase}.filtertype","message")
                ])));
                return DecodeServiceMessageResult<TMessage>.ProduceResult(filterResult);
            }
            var decodedMessage = await AfterMessageDecodeAsync<TMessage>(context, taskMessage!, message.ID, decodableMessage.MessageHeader, message.ReceivedTimestamp, DateTime.Now);
            return DecodeServiceMessageResult<TMessage>.ProduceResult(decodedMessage.Message, decodedMessage.MessageHeader);
        }
        #endregion

        #region OTEL
        private OpenTelemetryMiddleware? openTelemetryMiddleware;

        TContractConnection IMetricContractConnection<TContractConnection>.EnableOpenTelemetry(string activitySource, bool linkActivitiesAcrossSystems)
        {
            openTelemetryMiddleware = new(activitySource, linkActivitiesAcrossSystems);
            middleware.RegisterInjectionMiddleware<IBeforeEncodeMiddleware>(openTelemetryMiddleware, MiddlewareCollection.InjectionPositions.Pre);
            middleware.RegisterInjectionMiddleware<IAfterEncodeMiddleware>(openTelemetryMiddleware, MiddlewareCollection.InjectionPositions.Post);
            middleware.RegisterInjectionMiddleware<IBeforeDecodeMiddleware>(openTelemetryMiddleware, MiddlewareCollection.InjectionPositions.Pre);
            middleware.RegisterInjectionMiddleware<IAfterDecodeMiddleware>(openTelemetryMiddleware, MiddlewareCollection.InjectionPositions.Post);
            return (TContractConnection)(IBaseContractConnection)this;
        }


        protected Activity? StartActivity(string name, MessageHeader? messageHeader = null, IMessageServiceConnection? serviceConnection = null, string? connectionName = null, Activity? current = null)
            => openTelemetryMiddleware?.StartActivity(name, messageHeader, serviceConnection, connectionName, current);
        #endregion

        #region Metrics

        private MetricsMiddleware? metricsMiddleware;

        TContractConnection IMetricContractConnection<TContractConnection>.AddMetrics(Meter? meter, bool useInternal)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Enabling metrics on service connection with {Meter} and {UseInternal}", meter, useInternal);
            metricsMiddleware = new MetricsMiddleware(meter, messageContext, useInternal);
            middleware.RegisterInjectionMiddleware<IBeforeEncodeMiddleware>(metricsMiddleware, MiddlewareCollection.InjectionPositions.Pre);
            middleware.RegisterInjectionMiddleware<IAfterEncodeMiddleware>(metricsMiddleware, MiddlewareCollection.InjectionPositions.Post);
            middleware.RegisterInjectionMiddleware<IBeforeDecodeMiddleware>(metricsMiddleware, MiddlewareCollection.InjectionPositions.Pre);
            middleware.RegisterInjectionMiddleware<IAfterDecodeMiddleware>(metricsMiddleware, MiddlewareCollection.InjectionPositions.Post);
            return (TContractConnection)(IBaseContractConnection)this;
        }

        IContractMetric? IMetricContractConnection<TContractConnection>.GetSnapshot(bool sent)
            => metricsMiddleware?.GetSnapshot(sent);
        IContractMetric? IMetricContractConnection<TContractConnection>.GetSnapshot(Type messageType, bool sent)
            => metricsMiddleware?.GetSnapshot(messageType, sent);
        IContractMetric? IMetricContractConnection<TContractConnection>.GetSnapshot<TMessage>(bool sent)
            => metricsMiddleware?.GetSnapshot(typeof(TMessage), sent);
        IContractMetric? IMetricContractConnection<TContractConnection>.GetSnapshot(string channel, bool sent)
            => metricsMiddleware?.GetSnapshot(channel, sent);
        #endregion

        #region Subscriptions
        protected abstract ValueTask<ISubscription> CreateSubscriptionAsync<TMessage>(Func<IReceivedMessage<TMessage>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, bool synchronous, CancellationToken cancellationToken);

        protected async ValueTask<ISubscription> CreateSubscriptionAsync<TMessage>(IMessageFactory<TMessage> messageFactory, IMessageServiceConnection serviceConnection, Func<IReceivedMessage<TMessage>, ValueTask> messageReceived, Action<Exception> errorReceived,
            string? channel, string? group, bool synchronous, string? serviceConnectionName, MessageFilters<TMessage>? messageFilters, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Creating PubSub Subscription for {T} on {Channel} in {Group}.", typeof(TMessage), channel, group);
            var subscription = new PubSubSubscription<TMessage>(
                async (serviceMessage) =>
                {
                    using var activity = StartActivity(Constants.ConsumeActivityName, messageHeader: serviceMessage.Header, serviceConnection: serviceConnection, connectionName: serviceConnectionName);
                    try
                    {
                        var decodedResult = await DecodeServiceMessageAsync<TMessage>(ChannelMapper.MapTypes.PublishSubscription, messageFactory, serviceMessage, activity, messageFilters);
                        if (decodedResult.FilterResult == MessageFilterResult.Allow)
                            await messageReceived(new ReceivedMessage<TMessage>(serviceMessage.ID, decodedResult.Message!, decodedResult.Header!, serviceMessage.ReceivedTimestamp, DateTime.Now, activity));
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
                messageContext,
                channel: channel,
            group: group,
            synchronous: synchronous,
                logger: Logger);
            Logger?.LogInformationChecked("Establishing PubSub Subscription connection.");
            if (await subscription.EstablishSubscriptionAsync(serviceConnection, cancellationToken))
                return subscription;
            Logger?.LogInformationChecked("Establishment of PubSub Subscription connection failed.");
            throw new SubscriptionFailedException();
        }

        ValueTask<ISubscription> IBaseContractConnection.SubscribeAsync<TMessage>(Func<IReceivedMessage<TMessage>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Creating PubSub subscription for message type {T} on channel {Channel} in group {Group}", typeof(TMessage), channel, group);
            return CreateSubscriptionAsync<TMessage>(messageReceived, errorReceived, channel, group, ignoreMessageHeader, messageFilters, false, cancellationToken);
        }

        ValueTask<ISubscription> IBaseContractConnection.SubscribeAsync<TMessage>(Action<IReceivedMessage<TMessage>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Creating PubSub subscription for message type {T} on channel {Channel} in group {Group}", typeof(TMessage), channel, group);
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
            Logger?.LogDebugChecked("Creating QueryResponse subscription for message query {Q} and response {R} on channel {Channel} in group {Group}", typeof(TQuery), typeof(TQueryResponse), channel, group);
            return ProduceSubscribeQueryResponseAsync<TQuery, TQueryResponse>(messageReceived, errorReceived, channel, group, ignoreMessageHeader, false, messageFilters, cancellationToken);
        }

        ValueTask<ISubscription> IBaseContractConnection.SubscribeQueryResponseAsync<TQuery, TQueryResponse>(Func<IReceivedMessage<TQuery>, QueryResponseMessage<TQueryResponse>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TQuery>? messageFilters, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Creating QueryResponse subscription for message query {Q} and response {R} on channel {Channel} in group {Group}", typeof(TQuery), typeof(TQueryResponse), channel, group);
            return ProduceSubscribeQueryResponseAsync<TQuery, TQueryResponse>((msg) =>
            {
                var result = messageReceived(msg);
                return ValueTask.FromResult(result);
            }, errorReceived, channel, group, ignoreMessageHeader, true, messageFilters, cancellationToken);
        }
        #endregion

        #region PubSub
        protected async ValueTask<TransmissionResult> PublishMessageAsync<TMessage>(ServiceMessage serviceMessage, IMessageServiceConnection serviceConnection, Activity? activity, string? connectionName, CancellationToken cancellationToken)
        {
            var result = await ExecuteResilliantTransmissionAsync<TMessage>(
                (ct) => serviceConnection.PublishAsync(
                    serviceMessage,
                    ct
                ),
                activity,
                connectionName,
                serviceMessage.Channel,
                cancellationToken
            );
            OpenTelemetryMiddleware.AddMessagePublishedEvent(activity, serviceMessage, result, serviceConnection, connectionName);
            activity?.SetStatus(result.IsError ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            activity?.Stop();
            return result;
        }
        protected async ValueTask<IEnumerable<TransmissionResult>> BulkPublishAsync<TMessage>(IEnumerable<ServiceMessage> serviceMessages, IMessageServiceConnection serviceConnection, Activity? activity, CancellationToken cancellationToken, string? connectionName = null)
        {
            IEnumerable<TransmissionResult> result;
            using var scope = SetScope();
            Logger?.LogDebugChecked("Executing bulk publish");
            result = await ExecuteResilliantTransmissionAsync<TMessage>(
                serviceConnection.BulkPublishAsync,
                activity,
                connectionName,
                serviceMessages,
                cancellationToken
            );
            if (activity!=null)
            {
                foreach (var res in result)
                    activity.AddEvent(new(Constants.PublishBulkMessagesMessageEvent, tags: new([
                        new($"{OpenTelemetryMiddleware.KeyBase}.bulksupported",true),
                        new(OpenTelemetryMiddleware.MessageIdKey,res.ID),
                        OpenTelemetryMiddleware.CreateMessagePublishStatusTag(res),
                        OpenTelemetryMiddleware.CreateConnectionTypeTag(serviceConnection)
                    ])));
            }
            return result;
        }
        #endregion

        #region QueryResponse
        private readonly record struct InboxMessageResult(ServiceQueryResult? ServiceQueryResult, ErrorMessage? ErrorMessage);

        private async ValueTask<InboxMessageResult> ProcessInboxMessageAsync<TMessage>(string? connectionName, IInboxQueryableMessageServiceConnection inboxMessageServiceConnection, ServiceMessage serviceMessage, TimeSpan timeout, Activity? activity, CancellationToken cancellationToken)
        {
            using var scope = SetScope(serviceMessage.ID);
            Logger?.LogDebugChecked("Establishing an instance of Inbox Message style handling for a QueryResponse call on {ConnectionName}", connectionName);
            var messageID = Guid.NewGuid();
            Logger?.LogInformationChecked("Setting up Inbox Message listener with {CorrelationID}", messageID);
            if (!inboxSubscriptions.TryGetValue(connectionName??"DEFAULT", out var inboxSubscription))
            {
                Logger?.LogDebugChecked("Establishing new Inbox Subscription for {ConnectionName}", connectionName);
                inboxSubscription = await inboxMessageServiceConnection.EstablishInboxSubscriptionAsync(
                    async (message) =>
                    {
                        if (message.Acknowledge!=null)
                            await message.Acknowledge();
                        using var scope = SetScope(message.ID);
                        Logger?.LogInformationChecked("Attempting to process Inbox message with {CorrelationID}", message.CorrelationID);
                        if (inboxResponses.TryGetValue(message.CorrelationID, out var taskCompletionSource))
                        {
                            taskCompletionSource.TrySetResult(new(
                                message.ID,
                                message.Header,
                                message.MessageTypeID,
                                message.Data
                            ));
                        }
                    },
                    cancellationToken
                );
                inboxSubscriptions.TryAdd(connectionName ?? "DEFAULT", inboxSubscription);
            }
            var tcs = new TaskCompletionSource<ServiceQueryResult>();
            inboxResponses.TryAdd(messageID, tcs);
            using var token = new CancellationTokenSource();
            var reg = cancellationToken.Register(() => token.Cancel());
            token.Token.Register(async () =>
            {
                await reg.DisposeAsync();
                if (!tcs.Task.IsCompleted)
                {
                    using var scope = SetScope(serviceMessage.ID);
                    Logger?.LogDebugChecked("Inbox Query Message has timed out waiting for the response");
                    tcs.TrySetException(new QueryTimeoutException());
                }
            });
            token.CancelAfter(timeout);
            Logger?.LogInformationChecked("Transmitting Inbox Query request to underlying system with {CorrelationID} and being waiting on response", messageID);
            var result = await ExecuteResilliantTransmissionAsync<TMessage>(
                async (ct) => await inboxMessageServiceConnection.QueryAsync(serviceMessage, messageID, ct),
                activity,
                connectionName,
                serviceMessage.Channel,
                cancellationToken
            );
            OpenTelemetryMiddleware.AddMessagePublishedEvent(activity, serviceMessage, result, inboxMessageServiceConnection, connectionName);
            if (result.IsError)
            {
                if (!token.IsCancellationRequested)
                    await token.CancelAsync();
                Logger?.LogInformationChecked("Inbox Query tranmission failed cleaning up resources");
                inboxResponses.TryRemove(messageID, out _);
                return new(null, result.Error);
            }
            try
            {
                await tcs.Task.WaitAsync(cancellationToken);
            }
            catch (TaskCanceledException tce)
            {
                return new(null, new(tce, true));
            }
            finally
            {
                if (!token.IsCancellationRequested)
                    await token.CancelAsync();
                inboxResponses.TryRemove(messageID, out _);
            }
            return new(tcs.Task.Result, null);
        }
        protected async ValueTask<QueryResult<TQueryResult>> ProduceResultAsync<TQueryResult>(ServiceQueryResult queryResult, IMessageServiceConnection serviceConnection, string? serviceConnectionName, string responseChannel = "")
        {
            using var scope = SetScope(queryResult.ID);
            Logger?.LogDebugChecked("Attempting to produce a Query Result of {R} from the Service Message of the type {MessageTypeID}", typeof(TQueryResult), queryResult.MessageTypeID);
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
                Logger?.LogErrorChecked(qre, "A query response exception occured");
                result = new(
                    queryResult.ID,
                    queryResult.Header,
                    Result: default,
                    Error: new(qre)
                );
            }
            catch (Exception ex)
            {
                Logger?.LogErrorChecked(ex, "An error occured attempting to convert the Service Message of the type {MessageTypeID} to the Query Result of {R}", queryResult.MessageTypeID, typeof(TQueryResult));
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

        protected async ValueTask<QueryResult<TQueryResponse>> ExecuteQueryAsync<TQuery, TQueryResponse>(IMessageServiceConnection serviceConnection, ServiceMessage serviceMessage, Activity? activity, TimeSpan? timeout = null, string? responseChannel = null, string? connectionName = null, CancellationToken cancellationToken = new CancellationToken())
        {
            using var scope = SetScope(serviceMessage.ID);
            Logger?.LogDebugChecked("Attempting to execute a Query of {Q} with a response {R}", typeof(TQuery), typeof(TQueryResponse));
            var realTimeout = timeout??messageContext.QueryResponseTimeout<TQuery>();
            activity?.SetStatus(ActivityStatusCode.Ok);
            try
            {
                if (serviceConnection is IQueryResponseMessageServiceConnection queryableMessageServiceConnection)
                {
                    Logger?.LogInformationChecked("Executing a QueryResponse call on a QueryResponse service connection");
                    return await ExecuteResilliantTransmissionAsync<TQuery, TQueryResponse>(
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
                                return new QueryResult<TQueryResponse>(serviceMessage.ID, new([]), Error: new(te));
                            }
                            return await ProduceResultAsync<TQueryResponse>(
                                result,
                                serviceConnection,
                                connectionName
                            );
                        },
                        activity,
                        connectionName,
                        serviceMessage.Channel,
                        cancellationToken
                    );
                }
                else if (serviceConnection is IInboxQueryableMessageServiceConnection inboxMessageServiceConnection)
                {
                    Logger?.LogInformationChecked("Executing a QueryResponse call on an InboxQuery service connection");
                    var inboxResult = await ProcessInboxMessageAsync<TQuery>(connectionName, inboxMessageServiceConnection, serviceMessage, realTimeout??inboxMessageServiceConnection.DefaultTimeout, activity, cancellationToken);
                    if (inboxResult.ServiceQueryResult !=null)
                        return await ProduceResultAsync<TQueryResponse>(
                            inboxResult.ServiceQueryResult!,
                            serviceConnection,
                            connectionName
                        );
                    activity?.SetStatus(ActivityStatusCode.Error);
                    return new(serviceMessage.ID, new([]), Error: inboxResult.ErrorMessage);
                }
                Logger?.LogInformationChecked("Executing a QueryResponse call on a standard PubSub service connection using {ResponseChannel}", responseChannel);
                return await ProcessPubSubQuery<TQuery, TQueryResponse>(serviceConnection, connectionName, responseChannel, realTimeout, serviceMessage, activity, cancellationToken);
            }
            catch (Exception ex)
            {
                if (ex is QueryTimeoutException || ex is QueryExecutionFailedException)
                    activity?.SetStatus(ActivityStatusCode.Error);
                throw;
            }
        }

        protected async ValueTask<QueryResult<TQueryREsponse>> ProcessPubSubQuery<TQuery, TQueryREsponse>(IMessageServiceConnection serviceConnection, string? connectionName, string? responseChannel, TimeSpan? realTimeout, ServiceMessage serviceMessage, Activity? activity, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            responseChannel ??= messageContext.QueryResponseChannel<TQuery>();
            Logger?.LogInformationChecked("Attempting a QueryResponse call using PubSub style messaging, querying {Q}, expecting a response of {R} on {ResponseChannel}", typeof(TQuery), typeof(TQueryREsponse), responseChannel);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(responseChannel);
            var replyChannel = await MapChannel(ChannelMapper.MapTypes.QueryResponse, responseChannel!);
            Logger?.LogDebugChecked("QueryResponse reply channel mapped to {ReplyChannel}", replyChannel);
            var callID = Guid.NewGuid();
            Logger?.LogDebugChecked("Starting Response listener for Query over PubSub waiting on a message with {CallID}", callID);
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
            Logger?.LogDebugChecked("Transmitting Query request over PubSub");
            var result = await ExecuteResilliantTransmissionAsync<TQuery>(
                async (ct) => await serviceConnection.PublishAsync(msg, cancellationToken: ct),
                activity,
                connectionName,
                serviceMessage.Channel,
                cancellationToken
            );
            OpenTelemetryMiddleware.AddMessagePublishedEvent(activity, msg, result, serviceConnection, connectionName);
            if (result.IsError)
            {
                if (!token.IsCancellationRequested)
                    await token.CancelAsync();
                Logger?.LogDebugChecked("Inbox Query tranmission failed cleaning up resources");
                activity?.SetStatus(ActivityStatusCode.Error);
                return new(serviceMessage.ID, new([]), Error: result.Error);
            }
            try
            {
                Logger?.LogDebugChecked("Waiting on Query Response over PubSub");
                await tcs.Task.WaitAsync(cancellationToken);
                Logger?.LogDebugChecked("Query Response over PubSub recieved");
            }
            finally
            {
                if (!token.IsCancellationRequested)
                    await token.CancelAsync();
            }
            return await ProduceResultAsync<TQueryREsponse>(tcs.Task.Result, serviceConnection, connectionName, responseChannel: responseChannel);
        }
        protected async ValueTask<ISubscription> CreateSubscriptionAsync<TQuery, TQueryResponse>(IMessageFactory<TQuery> queryMessageFactory, IMessageFactory<TQueryResponse> responseMessageFactory, IMessageServiceConnection serviceConnection,
            Func<IReceivedMessage<TQuery>, ValueTask<QueryResponseMessage<TQueryResponse>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool synchronous, string? serviceConnectionName, MessageFilters<TQuery>? messageFilters, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogInformationChecked("Creating QueryResponse subscription for {Q} answering with {R} on {Channel} in {Group}", typeof(TQuery), typeof(TQueryResponse), channel, group);
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
                            return new(response, responseActivity, decodedResult.FilterResult);
                        }
                        catch
                        {
                            responseActivity?.SetStatus(ActivityStatusCode.Error);
                            throw;
                        }
                    }
                    return new(null, consumeActivity, decodedResult.FilterResult);
                },
                errorReceived,
                (originalChannel) => MapChannel(ChannelMapper.MapTypes.QuerySubscription, originalChannel),
                messageContext,
                channel: channel,
            group: group,
            synchronous: synchronous,
                logger: Logger);
            Logger?.LogDebugChecked("Establishing QueryResponse subscription");
            if (await subscription.EstablishSubscriptionAsync(serviceConnection, serviceConnectionName, cancellationToken))
                return subscription;
            Logger?.LogDebugChecked("Failed to establish subscription");
            throw new SubscriptionFailedException();
        }
        #endregion

        protected abstract ValueTask CloseAsync();
        async ValueTask IBaseContractConnection.CloseAsync()
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Closing contract connection");
            Logger?.LogInformationChecked("Closing all open inbox subscriptions");
            await Task.WhenAll([
                .. inboxSubscriptions.Values.Select(sub => sub.EndAsync().AsTask()),
                .. consumerSubscriptions.Select(sub=>sub.EndAsync().AsTask())
            ]);
            await CloseAsync();
        }
        protected abstract ValueTask InternalDisposeAsync();

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                await Task.WhenAll([
                    .. inboxSubscriptions.Values.Select(async(sub) => {
                        if (sub is IAsyncDisposable asyncDisposable)
                            await asyncDisposable.DisposeAsync();
                        else if (sub is IDisposable disposable)
                            disposable.Dispose();
                    }),
                    .. consumerSubscriptions.Select(async(sub)=>{
                        await sub.DisposeAsync();
                    })
                ]);
                consumerSubscriptions.Clear();
                inboxSubscriptions.Clear();
                await InternalDisposeAsync();
            }
            GC.SuppressFinalize(this);
        }
    }
}
