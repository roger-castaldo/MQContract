using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
using System.Diagnostics.Metrics;
using MQContract.Attributes;
using System.Reflection;

namespace MQContract.Connections
{
    internal abstract class AConnection<CC>(IMessageEncoder? defaultMessageEncoder = null,
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
        private readonly List<object> middleware = [new ChannelMappingMiddleware(channelMapper)];
        private readonly SemaphoreSlim inboxSemaphore = new(1, 1);
        private readonly Dictionary<Guid, TaskCompletionSource<ServiceQueryResult>> inboxResponses = [];
        private readonly Dictionary<string,IServiceSubscription> inboxSubscriptions = [];
        private IEnumerable<IMessageTypeFactory> typeFactories = [];
        protected ILogger? Logger => logger;

        protected IMessageFactory<T> GetMessageFactory<T>(uint? maxMessageBodySize,bool ignoreMessageHeader = false) where T : class
        {
            dataLock.Wait();
            var result = (IMessageFactory<T>?)typeFactories.FirstOrDefault(fact => fact.GetType().GetGenericArguments()[0] == typeof(T));
            dataLock.Release();
            if (result == null)
            {
                result = new MessageTypeFactory<T>(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, ignoreMessageHeader, maxMessageBodySize);
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

        private CC RegisterMiddleware(object element)
        {
            dataLock.Wait();
            middleware.Add(element);
            dataLock.Release();
            return (CC)(IBaseContractConnection)this;
        }

        private CC RegisterMiddlewareType(Type type)
            => RegisterMiddleware((serviceProvider == null ? Activator.CreateInstance(type) : ActivatorUtilities.CreateInstance(serviceProvider, type))!);

        CC IMetricContractConnection<CC>.RegisterMiddleware<T>()
            => RegisterMiddlewareType(typeof(T));

        CC IMetricContractConnection<CC>.RegisterMiddleware<T>(Func<T> constructInstance)
            => RegisterMiddleware(constructInstance());

        CC IMetricContractConnection<CC>.RegisterMiddleware<T, M>()
            => RegisterMiddlewareType(typeof(T));

        CC IMetricContractConnection<CC>.RegisterMiddleware<T, M>(Func<T> constructInstance)
            => RegisterMiddleware(constructInstance());

        private async ValueTask<(T message, string? channel, MessageHeader messageHeader)> BeforeMessageEncodeAsync<T>(IContext context, T message, string? channel, MessageHeader messageHeader)
            where T : class
        {
            IBeforeEncodeMiddleware[] genericHandlers;
            IBeforeEncodeSpecificTypeMiddleware<T>[] specificHandlers;
            lock (middleware)
            {
                genericHandlers = middleware.OfType<IBeforeEncodeMiddleware>().ToArray();
                specificHandlers = middleware.OfType<IBeforeEncodeSpecificTypeMiddleware<T>>().ToArray();
            }
            foreach (var handler in genericHandlers)
                (message, channel, messageHeader) = await handler.BeforeMessageEncodeAsync<T>(context, message, channel, messageHeader);
            foreach (var handler in specificHandlers)
                (message, channel, messageHeader) = await handler.BeforeMessageEncodeAsync(context, message, channel, messageHeader);
            return (message, channel, messageHeader);
        }

        private async ValueTask<ServiceMessage> AfterMessageEncodeAsync<T>(IContext context, ServiceMessage message)
        {
            IAfterEncodeMiddleware[] genericHandlers;
            lock (middleware)
            {
                genericHandlers = middleware.OfType<IAfterEncodeMiddleware>().ToArray();
            }
            foreach (var handler in genericHandlers)
                message = await handler.AfterMessageEncodeAsync(typeof(T), context, message);
            return message;
        }

        private async ValueTask<(MessageHeader messageHeader, ReadOnlyMemory<byte> data)> BeforeMessageDecodeAsync(IContext context, string id, MessageHeader messageHeader, string messageTypeID, string messageChannel, ReadOnlyMemory<byte> data)
        {
            IBeforeDecodeMiddleware[] genericHandlers;
            lock (middleware)
            {
                genericHandlers = middleware.OfType<IBeforeDecodeMiddleware>().ToArray();
            }
            foreach (var handler in genericHandlers)
                (messageHeader, data) = await handler.BeforeMessageDecodeAsync(context, id, messageHeader, messageTypeID, messageChannel, data);
            return (messageHeader, data);
        }

        private async ValueTask<(T message, MessageHeader messageHeader)> AfterMessageDecodeAsync<T>(IContext context, T message, string ID, MessageHeader messageHeader, DateTime receivedTimestamp, DateTime processedTimeStamp)
            where T : class
        {
            IAfterDecodeMiddleware[] genericHandlers;
            IAfterDecodeSpecificTypeMiddleware<T>[] specificHandlers;
            lock (middleware)
            {
                genericHandlers = middleware.OfType<IAfterDecodeMiddleware>().ToArray();
                specificHandlers = middleware.OfType<IAfterDecodeSpecificTypeMiddleware<T>>().ToArray();
            }
            foreach (var handler in genericHandlers)
                (message, messageHeader) = await handler.AfterMessageDecodeAsync<T>(context, message, ID, messageHeader, receivedTimestamp, processedTimeStamp);
            foreach (var handler in specificHandlers)
                (message, messageHeader) = await handler.AfterMessageDecodeAsync(context, message, ID, messageHeader, receivedTimestamp, processedTimeStamp);
            return (message, messageHeader);
        }

        protected async ValueTask<ServiceMessage> ProduceServiceMessageAsync<T>(ChannelMapper.MapTypes mapType, IMessageFactory<T> messageFactory, T message, bool ignoreChannel, string? channel = null, MessageHeader? messageHeader = null)
            where T : class
        {
            var context = new Context(mapType);
            (message, channel, messageHeader) = await BeforeMessageEncodeAsync<T>(context, message, channel??messageFactory.MessageChannel, messageHeader??new([]));
            return await AfterMessageEncodeAsync<T>(context,
                await messageFactory.ConvertMessageAsync(message, ignoreChannel, channel, messageHeader)
            );
        }
        protected async ValueTask<(T message, MessageHeader header)> DecodeServiceMessageAsync<T>(ChannelMapper.MapTypes mapType, IMessageFactory<T> messageFactory, ReceivedServiceMessage message)
            where T : class
        {
            var context = new Context(mapType);
            (var messageHeader, var data) = await BeforeMessageDecodeAsync(context, message.ID, message.Header, message.MessageTypeID, message.Channel, message.Data);
            var taskMessage = await messageFactory.ConvertMessageAsync(logger, new ReceivedServiceMessage(message.ID, message.MessageTypeID, message.Channel, messageHeader, data, message.Acknowledge))
                                ??throw new InvalidCastException($"Unable to convert incoming message {message.MessageTypeID} to {typeof(T).FullName}");
            return await AfterMessageDecodeAsync<T>(context, taskMessage!, message.ID, messageHeader, message.ReceivedTimestamp, DateTime.Now);
        }
        #endregion

        #region Metrics

        CC IMetricContractConnection<CC>.AddMetrics(Meter? meter, bool useInternal)
        {
            dataLock.Wait();
            middleware.Insert(0, new MetricsMiddleware(meter, useInternal));
            dataLock.Release();
            return (CC)(IBaseContractConnection)this;
        }

        private MetricsMiddleware? MetricsMiddleware
        {
            get
            {
                MetricsMiddleware? metricsMiddleware;
                lock (middleware)
                {
                    metricsMiddleware = middleware.OfType<MetricsMiddleware>().FirstOrDefault();
                }
                return metricsMiddleware;
            }
        }

        IContractMetric? IMetricContractConnection<CC>.GetSnapshot(bool sent)
            => MetricsMiddleware?.GetSnapshot(sent);
        IContractMetric? IMetricContractConnection<CC>.GetSnapshot(Type messageType, bool sent)
            => MetricsMiddleware?.GetSnapshot(messageType, sent);
        IContractMetric? IMetricContractConnection<CC>.GetSnapshot<T>(bool sent)
            => MetricsMiddleware?.GetSnapshot(typeof(T), sent);
        IContractMetric? IMetricContractConnection<CC>.GetSnapshot(string channel, bool sent)
            => MetricsMiddleware?.GetSnapshot(channel, sent);
        #endregion

        #region Subscriptions
        protected abstract ValueTask<ISubscription> CreateSubscriptionAsync<T>(Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, CancellationToken cancellationToken)
            where T : class;

        ValueTask<ISubscription> IBaseContractConnection.SubscribeAsync<T>(Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken) where T : class
            => CreateSubscriptionAsync<T>(messageReceived, errorReceived, channel, group, ignoreMessageHeader, false, cancellationToken);

        ValueTask<ISubscription> IBaseContractConnection.SubscribeAsync<T>(Action<IReceivedMessage<T>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken) where T : class
            => CreateSubscriptionAsync<T>((msg) =>
            {
                messageReceived(msg);
                return ValueTask.CompletedTask;
            },
            errorReceived, channel, group, ignoreMessageHeader, true, cancellationToken);

        protected abstract ValueTask<ISubscription> ProduceSubscribeQueryResponseAsync<Q, R>(Func<IReceivedMessage<Q>, ValueTask<QueryResponseMessage<R>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, CancellationToken cancellationToken)
            where Q : class
            where R : class;

        ValueTask<ISubscription> IBaseContractConnection.SubscribeQueryAsyncResponseAsync<Q, R>(Func<IReceivedMessage<Q>, ValueTask<QueryResponseMessage<R>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => ProduceSubscribeQueryResponseAsync<Q, R>(messageReceived, errorReceived, channel, group, ignoreMessageHeader, false, cancellationToken);

        ValueTask<ISubscription> IBaseContractConnection.SubscribeQueryResponseAsync<Q, R>(Func<IReceivedMessage<Q>, QueryResponseMessage<R>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => ProduceSubscribeQueryResponseAsync<Q, R>((msg) =>
            {
                var result = messageReceived(msg);
                return ValueTask.FromResult(result);
            }, errorReceived, channel, group, ignoreMessageHeader, true, cancellationToken);
        #endregion

        #region PubSub
        protected static async ValueTask<IEnumerable<TransmissionResult>> BulkPublishAsync(IEnumerable<ServiceMessage> serviceMessages,IMessageServiceConnection serviceConnection,CancellationToken cancellationToken)
        {
            if (serviceConnection is IBulkPublishableMessageServiceConnection bulkPublishableMessageServiceConnection)
                return await bulkPublishableMessageServiceConnection.BulkPublishAsync(serviceMessages, cancellationToken);
            else
                return await serviceMessages
                    .WhenAll(message => serviceConnection.PublishAsync(message, cancellationToken));
        }

        protected async ValueTask<ISubscription> CreateSubscriptionAsync<T>(IMessageFactory<T> messageFactory, IMessageServiceConnection serviceConnection, Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool synchronous, CancellationToken cancellationToken)
            where T : class
        {
            var subscription = new PubSubSubscription<T>(
                async (serviceMessage) =>
                {
                    (var taskMessage, var messageHeader) = await DecodeServiceMessageAsync<T>(ChannelMapper.MapTypes.PublishSubscription, messageFactory, serviceMessage);
                    await messageReceived(new ReceivedMessage<T>(serviceMessage.ID, taskMessage!, messageHeader, serviceMessage.ReceivedTimestamp, DateTime.Now));
                },
                errorReceived,
                (originalChannel) => MapChannel(ChannelMapper.MapTypes.PublishSubscription, originalChannel)!,
                channel: channel,
            group: group,
            synchronous: synchronous,
                logger: Logger);
            if (await subscription.EstablishSubscriptionAsync(serviceConnection, cancellationToken))
                return subscription;
            throw new SubscriptionFailedException();
        }
        #endregion

        #region QueryResponse
        private async ValueTask<ServiceQueryResult> ProcessInboxMessageAsync(string connectionName,IInboxQueryableMessageServiceConnection inboxMessageServiceConnection, ServiceMessage serviceMessage, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var messageID = Guid.NewGuid();
            await inboxSemaphore.WaitAsync(cancellationToken);
            if (!inboxSubscriptions.TryGetValue(connectionName, out var inboxSubscription))
            {
                inboxSubscription = await inboxMessageServiceConnection.EstablishInboxSubscriptionAsync(
                    async (message) =>
                    {
                        await inboxSemaphore.WaitAsync();
                        if (message.Acknowledge!=null)
                            await message.Acknowledge();
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
                inboxSubscriptions.Add(connectionName, inboxSubscription);
            }
            var tcs = new TaskCompletionSource<ServiceQueryResult>();
            inboxResponses.Add(messageID, tcs);
            inboxSemaphore.Release();
            using var token = new CancellationTokenSource();
            var reg = cancellationToken.Register(() => token.Cancel());
            token.Token.Register(async () => {
                await reg.DisposeAsync();
                if (!tcs.Task.IsCompleted)
                    tcs.TrySetException(new QueryTimeoutException());
            });
            token.CancelAfter(timeout);
            var result = await inboxMessageServiceConnection.QueryAsync(serviceMessage, messageID, cancellationToken);
            if (result.IsError)
            {
                await inboxSemaphore.WaitAsync();
                inboxResponses.Remove(messageID);
                inboxSemaphore.Release();
                throw new QuerySubmissionFailedException(result.Error!);
            }
            try
            {
                await tcs.Task.WaitAsync(cancellationToken);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                    await token.CancelAsync();
                await inboxSemaphore.WaitAsync();
                inboxResponses.Remove(messageID);
                inboxSemaphore.Release();
            }
            return tcs.Task.Result;
        }
        protected async ValueTask<QueryResult<R>> ProduceResultAsync<R>(uint? maxMessageBodySize,ServiceQueryResult queryResult) where R : class
        {
            QueryResult<R> result;
            try
            {
                (var resultMessage, var messageHeader) = await DecodeServiceMessageAsync<R>(ChannelMapper.MapTypes.QueryResponse, GetMessageFactory<R>(maxMessageBodySize, true), new(queryResult.ID, queryResult.MessageTypeID, string.Empty, queryResult.Header, queryResult.Data));
                result = new QueryResult<R>(
                    queryResult.ID,
                    messageHeader,
                    Result: resultMessage
                );
            }
            catch (QueryResponseException qre)
            {
                return new(
                    queryResult.ID,
                    queryResult.Header,
                    Result: default,
                    Error: qre.Message
                );
            }
            catch (Exception ex)
            {
                return new(
                    queryResult.ID,
                    queryResult.Header,
                    Result: default,
                    Error: ex.Message
                );
            }
            return result;
        }

        protected async ValueTask<QueryResult<R>> ExecuteQueryAsync<Q,R>(IMessageServiceConnection serviceConnection, ServiceMessage serviceMessage, TimeSpan? timeout = null, string? responseChannel = null,string connectionName = "DEFAULT", CancellationToken cancellationToken = new CancellationToken())
                where Q : class
                where R : class
        {
            var realTimeout = timeout??typeof(Q).GetCustomAttribute<MessageResponseTimeoutAttribute>()?.TimeSpanValue;
            if (serviceConnection is IQueryResponseMessageServiceConnection queryableMessageServiceConnection)
                return await ProduceResultAsync<R>(
                    serviceConnection.MaxMessageBodySize,
                    await queryableMessageServiceConnection.QueryAsync(
                        serviceMessage,
                        realTimeout??queryableMessageServiceConnection.DefaultTimeout,
                        cancellationToken
                    )
                );
            else if (serviceConnection is IInboxQueryableMessageServiceConnection inboxMessageServiceConnection)
                return await ProduceResultAsync<R>(
                    serviceConnection.MaxMessageBodySize,
                    await ProcessInboxMessageAsync(connectionName,inboxMessageServiceConnection, serviceMessage, realTimeout??inboxMessageServiceConnection.DefaultTimeout, cancellationToken)
                );
            return await ProcessPubSubQuery<Q, R>(serviceConnection, responseChannel, realTimeout, serviceMessage, cancellationToken);
        }

        protected async ValueTask<QueryResult<R>> ProcessPubSubQuery<Q, R>(IMessageServiceConnection serviceConnection, string? responseChannel, TimeSpan? realTimeout, ServiceMessage serviceMessage, CancellationToken cancellationToken)
            where Q : class
            where R : class
        {
            responseChannel ??=typeof(Q).GetCustomAttribute<QueryResponseChannelAttribute>()?.Name;
            ArgumentNullException.ThrowIfNullOrWhiteSpace(responseChannel);
            var replyChannel = await MapChannel(ChannelMapper.MapTypes.QueryResponse, responseChannel!);
            var callID = Guid.NewGuid();
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
            await serviceConnection.PublishAsync(msg, cancellationToken: cancellationToken);
            try
            {
                await tcs.Task.WaitAsync(cancellationToken);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                    await token.CancelAsync();
            }
            return await ProduceResultAsync<R>(serviceConnection.MaxMessageBodySize, tcs.Task.Result);
        }
        protected async ValueTask<ISubscription> CreateSubscriptionAsync<Q, R>(IMessageFactory<Q> queryMessageFactory,IMessageFactory<R> responseMessageFactory, IMessageServiceConnection serviceConnection,
            Func<IReceivedMessage<Q>, ValueTask<QueryResponseMessage<R>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool synchronous, CancellationToken cancellationToken)
            where Q : class
            where R : class
        {
            var subscription = new QueryResponseSubscription<Q>(
                async (message, replyChannel) =>
                {
                    (var taskMessage, var messageHeader) = await DecodeServiceMessageAsync<Q>(
                        ChannelMapper.MapTypes.QuerySubscription,
                        queryMessageFactory,
                        message
                    );
                    var result = await messageReceived(new ReceivedMessage<Q>(message.ID, taskMessage!, messageHeader, message.ReceivedTimestamp, DateTime.Now));
                    return await ProduceServiceMessageAsync<R>(
                        ChannelMapper.MapTypes.QueryResponse,
                        responseMessageFactory,
                        result.Message,
                        true,
                        replyChannel,
                        new(result.Headers)
                    );
                },
                errorReceived,
                (originalChannel) => MapChannel(ChannelMapper.MapTypes.QuerySubscription, originalChannel),
                channel: channel,
            group: group,
            synchronous: synchronous,
                logger: Logger);
            if (await subscription.EstablishSubscriptionAsync(serviceConnection, cancellationToken))
                return subscription;
            throw new SubscriptionFailedException();
        }
        #endregion

        protected abstract ValueTask CloseAsync();
        async ValueTask IBaseContractConnection.CloseAsync()
        {
            await inboxSemaphore.WaitAsync();
            foreach (var key in inboxSubscriptions.Keys)
            {
                var inboxSubscription = inboxSubscriptions[key];
                await inboxSubscription.EndAsync();
            }
            inboxSemaphore.Release();
            await CloseAsync();
        }

        protected abstract void InternalDispose();
        protected abstract ValueTask InternalDisposeAsync();

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    inboxSemaphore.Wait();
                    foreach (var key in inboxSubscriptions.Keys)
                    {
                        var inboxSubscription = inboxSubscriptions[key];
                        if (inboxSubscription is IAsyncDisposable asyncSubDisposable)
                            asyncSubDisposable.DisposeAsync().AsTask().Wait();
                        else if (inboxSubscription is IDisposable subDisposable)
                            subDisposable.Dispose();
                    }
                    inboxSubscriptions.Clear();
                    inboxSemaphore.Release();
                    inboxSemaphore.Dispose();
                    InternalDispose();
                }
                dataLock.Dispose();
                disposedValue =true;
            }
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await inboxSemaphore.WaitAsync();
            foreach(var key in inboxSubscriptions.Keys)
            {
                var inboxSubscription = inboxSubscriptions[key];
                if (inboxSubscription is IAsyncDisposable asyncSubDisposable)
                    await asyncSubDisposable.DisposeAsync();
                else if (inboxSubscription is IDisposable subDisposable)
                    subDisposable.Dispose();
            }
            inboxSubscriptions.Clear();
            inboxSemaphore.Release();
            inboxSemaphore.Dispose();
            await InternalDisposeAsync();
            Dispose(false);
            GC.SuppressFinalize(this);
        }
    }
}
