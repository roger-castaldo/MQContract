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
using System.Reflection;

namespace MQContract
{
    /// <summary>
    /// The primary ContractConnection item which implements IContractConnection
    /// </summary>
    public sealed partial class ContractConnection
                : IContractConnection
    {
        private readonly Guid indentifier = Guid.NewGuid();
        private readonly SemaphoreSlim dataLock = new(1, 1);
        private readonly IMessageServiceConnection serviceConnection;
        private readonly IMessageEncoder? defaultMessageEncoder;
        private readonly IMessageEncryptor? defaultMessageEncryptor;
        private readonly IServiceProvider? serviceProvider;
        private readonly ILogger? logger;
        private readonly ChannelMapper? channelMapper;
        private readonly List<object> middleware;
        private IEnumerable<IMessageTypeFactory> typeFactories = [];
        private bool disposedValue;

        private ContractConnection(IMessageServiceConnection serviceConnection,
        IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null)
        {
            this.serviceConnection = serviceConnection;
            this.defaultMessageEncoder = defaultMessageEncoder;
            this.defaultMessageEncryptor= defaultMessageEncryptor;
            this.serviceProvider = serviceProvider;
            this.logger=logger;
            this.channelMapper=channelMapper;
            this.middleware= [new ChannelMappingMiddleware(this.channelMapper)];
        }

        /// <summary>
        /// This is the call used to create an instance of a Contract Connection which will return the Interface
        /// </summary>
        /// <param name="serviceConnection">The service connection implementation to use for the underlying message requests.</param>
        /// <param name="defaultMessageEncoder">A default message encoder implementation if desired.  If there is no specific encoder for a given type, this encoder would be called.  The built in default being used dotnet Json serializer.</param>
        /// <param name="defaultMessageEncryptor">A default message encryptor implementation if desired.  If there is no specific encryptor </param>
        /// <param name="serviceProvider">A service prodivder instance supplied in the case that dependency injection might be necessary</param>
        /// <param name="logger">An instance of a logger if logging is desired</param>
        /// <param name="channelMapper">An instance of a ChannelMapper used to translate channels from one instance to another based on class channel attributes or supplied channels if necessary.
        /// For example, it might be necessary for a Nats.IO instance when you are trying to read from a stored message stream that is comprised of another channel or set of channels
        /// </param>
        /// <returns>An instance of IContractConnection</returns>
        public static IContractConnection Instance(IMessageServiceConnection serviceConnection,
        IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null)
            => new ContractConnection(serviceConnection,defaultMessageEncoder,defaultMessageEncryptor,serviceProvider,logger, channelMapper);

        private IMessageFactory<T> GetMessageFactory<T>(bool ignoreMessageHeader = false) where T : class
        {
            dataLock.Wait();
            var result = (IMessageFactory<T>?)typeFactories.FirstOrDefault(fact => fact.GetType().GetGenericArguments()[0]==typeof(T));
            dataLock.Release();
            if (result==null)
            {
                result = new MessageTypeFactory<T>(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, ignoreMessageHeader, serviceConnection.MaxMessageBodySize);
                dataLock.Wait();
                if (!typeFactories.Any(fact => fact.GetType().GetGenericArguments()[0]==typeof(T) && fact.IgnoreMessageHeader==ignoreMessageHeader))
                    typeFactories = typeFactories.Concat([(IMessageTypeFactory)result]);
                dataLock.Release();
            }
            return result;
        }

        private ValueTask<string> MapChannel(ChannelMapper.MapTypes mapType, string originalChannel)
            => channelMapper?.MapChannel(mapType, originalChannel)??ValueTask.FromResult<string>(originalChannel);

        private async ValueTask<(IContext context, ServiceMessage)> ProduceServiceMessage<T>(ChannelMapper.MapTypes mapType, T message, string? channel = null, MessageHeader? messageHeader = null) where T : class
        {
            var factory = GetMessageFactory<T>();
            var context = new Context(mapType);
            (message, channel, messageHeader) = await BeforeMessageEncodeAsync<T>(context, message, channel??factory.MessageChannel, messageHeader??new([]));
            var serviceMessage = await AfterMessageEncodeAsync<T>(context,
                await factory.ConvertMessageAsync(message,false, channel, messageHeader)
            );
            return (context, serviceMessage);
        }

        private async ValueTask<ISubscription> CreateSubscriptionAsync<T>(Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, CancellationToken cancellationToken)
            where T : class
        {
            var messageFactory = GetMessageFactory<T>(ignoreMessageHeader);
            var subscription = new PubSubSubscription<T>(
                async (serviceMessage) =>
                {
                    var context = new Context(ChannelMapper.MapTypes.PublishSubscription);
                    (MessageHeader messageHeader, ReadOnlyMemory<byte> data)  = await BeforeMessageDecodeAsync(context, serviceMessage.ID, serviceMessage.Header, serviceMessage.MessageTypeID, serviceMessage.Channel, serviceMessage.Data);
                    var taskMessage = await messageFactory.ConvertMessageAsync(logger, new ServiceMessage(serviceMessage.ID, serviceMessage.MessageTypeID, serviceMessage.Channel, messageHeader, data))
                        ??throw new InvalidCastException($"Unable to convert incoming message {serviceMessage.MessageTypeID} to {typeof(T).FullName}");
                    (taskMessage, messageHeader) = await AfterMessageDecodeAsync<T>(context, taskMessage, serviceMessage.ID, messageHeader, serviceMessage.ReceivedTimestamp, DateTime.Now);
                    await messageReceived(new ReceivedMessage<T>(serviceMessage.ID, taskMessage!, messageHeader, serviceMessage.ReceivedTimestamp, DateTime.Now));
                },
                errorReceived,
                (originalChannel) => MapChannel(ChannelMapper.MapTypes.PublishSubscription, originalChannel)!,
                channel: channel,
                group: group,
                synchronous: synchronous,
                logger: logger);
            if (await subscription.EstablishSubscriptionAsync(serviceConnection, cancellationToken))
                return subscription;
            throw new SubscriptionFailedException();
        }

        private async ValueTask<QueryResult<R>> ExecuteQueryAsync<Q, R>(Q message, TimeSpan? timeout = null, string? channel = null, string? responseChannel = null, MessageHeader? messageHeader = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class
            where R : class
        {
            var realTimeout = timeout??typeof(Q).GetCustomAttribute<MessageResponseTimeoutAttribute>()?.TimeSpanValue;
            (var context, var serviceMessage) = await ProduceServiceMessage<Q>(ChannelMapper.MapTypes.Query, message, channel: channel, messageHeader: messageHeader);
            if (serviceConnection is IQueryableMessageServiceConnection queryableMessageServiceConnection)
                return await ProduceResultAsync<R>(
                    context,
                    await queryableMessageServiceConnection.QueryAsync(
                        serviceMessage,
                        realTimeout??queryableMessageServiceConnection.DefaultTimout,
                        cancellationToken
                    )
                );
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
            return await ProduceResultAsync<R>(context, tcs.Task.Result);
        }
        private async ValueTask<QueryResult<R>> ProduceResultAsync<R>(IContext context, ServiceQueryResult queryResult) where R : class
        {
            var receivedTime = DateTime.Now;
            (var messageHeader, var data) = await BeforeMessageDecodeAsync(context, queryResult.ID, queryResult.Header, queryResult.MessageTypeID, string.Empty, queryResult.Data);
            QueryResult<R> result;
            try
            {
                result = new QueryResult<R>(
                    queryResult.ID,
                    messageHeader,
                    Result: await GetMessageFactory<R>(true).ConvertMessageAsync(logger, new ServiceQueryResult(queryResult.ID, messageHeader, queryResult.MessageTypeID, data))
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
            (var decodedResult, var decodedHeader) = await AfterMessageDecodeAsync<R>(context, result.Result!, queryResult.ID, result.Header, receivedTime, DateTime.Now);
            return new QueryResult<R>(result.ID, decodedHeader, decodedResult);
        }
        private async ValueTask<ISubscription> ProduceSubscribeQueryResponseAsync<Q, R>(Func<IReceivedMessage<Q>, ValueTask<QueryResponseMessage<R>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, CancellationToken cancellationToken)
            where Q : class
            where R : class
        {
            var queryMessageFactory = GetMessageFactory<Q>(ignoreMessageHeader);
            var responseMessageFactory = GetMessageFactory<R>();
            var subscription = new QueryResponseSubscription<Q, R>(
                async (message,replyChannel) =>
                {
                    var context = new Context(ChannelMapper.MapTypes.QuerySubscription);
                    (var messageHeader, var data) = await BeforeMessageDecodeAsync(context, message.ID, message.Header, message.MessageTypeID, message.Channel, message.Data);
                    var taskMessage = await queryMessageFactory.ConvertMessageAsync(logger, new ReceivedServiceMessage(message.ID, message.MessageTypeID, message.Channel, messageHeader, data, message.Acknowledge))
                                        ??throw new InvalidCastException($"Unable to convert incoming message {message.MessageTypeID} to {typeof(Q).FullName}");
                    (taskMessage, messageHeader) = await AfterMessageDecodeAsync<Q>(context, taskMessage!, message.ID, messageHeader, message.ReceivedTimestamp, DateTime.Now);
                    var result = await messageReceived(new ReceivedMessage<Q>(message.ID, taskMessage, messageHeader, message.ReceivedTimestamp, DateTime.Now));
                    context = new Context(ChannelMapper.MapTypes.QueryResponse);
                    (var resultMessage, var resultChannel, var resultHeader) = await BeforeMessageEncodeAsync<R>(context, result.Message, replyChannel, message.Header);
                    var encodedMessage = await responseMessageFactory.ConvertMessageAsync(resultMessage,true, resultChannel??replyChannel, resultHeader);
                    return await AfterMessageEncodeAsync<R>(context, encodedMessage);
                },
                errorReceived,
                (originalChannel) => MapChannel(ChannelMapper.MapTypes.QuerySubscription, originalChannel),
                channel: channel,
                group: group,
                synchronous: synchronous,
                logger: logger);
            if (await subscription.EstablishSubscriptionAsync(serviceConnection, cancellationToken))
                return subscription;
            throw new SubscriptionFailedException();
        }

        ValueTask<PingResult> IContractConnection.PingAsync()
            => (serviceConnection is IPingableMessageServiceConnection pingableService ? pingableService.PingAsync() : throw new NotSupportedException("The underlying service does not support Ping"));

        async ValueTask<TransmissionResult> IContractConnection.PublishAsync<T>(T message, string? channel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            (_, var serviceMessage) = await ProduceServiceMessage<T>(ChannelMapper.MapTypes.Publish, message, channel: channel, messageHeader: messageHeader);
            return await serviceConnection.PublishAsync(
                serviceMessage,
                cancellationToken
            );
        }

        ValueTask<ISubscription> IContractConnection.SubscribeAsync<T>(Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken) where T : class
            => CreateSubscriptionAsync<T>(messageReceived, errorReceived, channel, group, ignoreMessageHeader,false, cancellationToken);

        ValueTask<ISubscription> IContractConnection.SubscribeAsync<T>(Action<IReceivedMessage<T>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken) where T : class
            => CreateSubscriptionAsync<T>((msg) =>
            {
                messageReceived(msg);
                return ValueTask.CompletedTask;
            }, 
            errorReceived, channel, group, ignoreMessageHeader, true, cancellationToken);

        async ValueTask<QueryResult<R>> IContractConnection.QueryAsync<Q, R>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
            => await ExecuteQueryAsync<Q, R>(message, timeout: timeout, channel: channel,responseChannel:responseChannel, messageHeader: messageHeader, cancellationToken: cancellationToken);

        async ValueTask<QueryResult<object>> IContractConnection.QueryAsync<Q>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader,
            CancellationToken cancellationToken)
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var responseType = (typeof(Q).GetCustomAttribute<QueryResponseTypeAttribute>(false)?.ResponseType)??throw new UnknownResponseTypeException("ResponseType", typeof(Q));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            var methodInfo = typeof(ContractConnection).GetMethod(nameof(ContractConnection.ExecuteQueryAsync), BindingFlags.NonPublic | BindingFlags.Instance)!.MakeGenericMethod(typeof(Q), responseType!);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            dynamic? queryResult;
            try
            {
                queryResult = (dynamic?)await Utility.InvokeMethodAsync(
                    methodInfo,
                    this, [
                        message,
                    timeout,
                    channel,
                    responseChannel,
                    messageHeader,
                    cancellationToken
                    ]
                );
            }catch(TimeoutException)
            {
                throw new QueryTimeoutException();
            }
            return new QueryResult<object>(queryResult?.ID??string.Empty, queryResult?.Header??new MessageHeader([]), queryResult?.Result, queryResult?.Error);
        }

        ValueTask<ISubscription> IContractConnection.SubscribeQueryAsyncResponseAsync<Q, R>(Func<IReceivedMessage<Q>, ValueTask<QueryResponseMessage<R>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => ProduceSubscribeQueryResponseAsync<Q,R>(messageReceived, errorReceived, channel, group, ignoreMessageHeader,false, cancellationToken);

        ValueTask<ISubscription> IContractConnection.SubscribeQueryResponseAsync<Q, R>(Func<IReceivedMessage<Q>, QueryResponseMessage<R>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        => ProduceSubscribeQueryResponseAsync<Q, R>((msg) =>
            {
                var result = messageReceived(msg);
                return ValueTask.FromResult(result);
            }, errorReceived, channel, group, ignoreMessageHeader, true, cancellationToken);

        ValueTask IContractConnection.CloseAsync()
            => serviceConnection?.CloseAsync()??ValueTask.CompletedTask;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (serviceConnection is IDisposable disposable)
                        disposable.Dispose();
                    else if (serviceConnection is IAsyncDisposable asyncDisposable)
                        asyncDisposable.DisposeAsync().AsTask().Wait();
                }
                disposedValue=true;
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
            if (serviceConnection is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(true);
            else if (serviceConnection is IDisposable disposable)
                disposable.Dispose();

            Dispose(false);
            GC.SuppressFinalize(this);
        }
    }
}
