﻿using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Factories;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Factories;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using MQContract.Subscriptions;
using System.Reflection;

namespace MQContract
{
    /// <summary>
    /// This is the primary class for this library and is used to create a Contract style connection between systems using the underlying service connection layer.
    /// </summary>
    /// <param name="serviceConnection">The service connection implementation to use for the underlying message requests.</param>
    /// <param name="defaultMessageEncoder">A default message encoder implementation if desired.  If there is no specific encoder for a given type, this encoder would be called.  The built in default being used dotnet Json serializer.</param>
    /// <param name="defaultMessageEncryptor">A default message encryptor implementation if desired.  If there is no specific encryptor </param>
    /// <param name="serviceProvider">A service prodivder instance supplied in the case that dependency injection might be necessary</param>
    /// <param name="logger">An instance of a logger if logging is desired</param>
    /// <param name="channelMapper">An instance of a ChannelMapper used to translate channels from one instance to another based on class channel attributes or supplied channels if necessary.
    /// For example, it might be necessary for a Nats.IO instance when you are trying to read from a stored message stream that is comprised of another channel or set of channels
    /// </param>
    public sealed class ContractConnection(IMessageServiceConnection serviceConnection,
        IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null)
                : IContractConnection
    {
        private readonly Guid Indentifier = Guid.NewGuid();
        private readonly SemaphoreSlim dataLock = new(1, 1);
        private IEnumerable<IMessageTypeFactory> typeFactories = [];
        private bool disposedValue;

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

        /// <summary>
        /// Called to execute a ping against the service layer
        /// </summary>
        /// <returns>The ping result from the service layer, if supported</returns>
        public ValueTask<PingResult> PingAsync()
            => (serviceConnection is IPingableMessageServiceConnection pingableService ? pingableService.PingAsync() : throw new NotSupportedException("The underlying service does not support Ping"));

        /// <summary>
        /// Called to publish a message out into the service layer in the Pub/Sub style
        /// </summary>
        /// <typeparam name="T">The type of message to publish</typeparam>
        /// <param name="message">The instance of the message to publish</param>
        /// <param name="channel">Used to override the MessageChannelAttribute from the class or to specify a channel to transmit the message on</param>
        /// <param name="messageHeader">A message header to be sent across with the message</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// 
        /// <returns>An instance of the TransmissionResult record to indicate success or failure and an ID</returns>
        public async ValueTask<TransmissionResult> PublishAsync<T>(T message, string? channel = null, MessageHeader? messageHeader = null, CancellationToken cancellationToken = new CancellationToken())
            where T : class
            => await serviceConnection.PublishAsync(
                await ProduceServiceMessage<T>(ChannelMapper.MapTypes.Publish, message, channel: channel, messageHeader: messageHeader),
                cancellationToken
            );

        private async ValueTask<ServiceMessage> ProduceServiceMessage<T>(ChannelMapper.MapTypes mapType,T message, string? channel = null, MessageHeader? messageHeader = null) where T : class
            => await GetMessageFactory<T>().ConvertMessageAsync(message, channel, messageHeader,(originalChannel)=>MapChannel(mapType,originalChannel)!);

        /// <summary>
        /// Called to establish a Subscription in the sevice layer for the Pub/Sub style messaging processing messages asynchronously
        /// </summary>
        /// <typeparam name="T">The type of message to listen for</typeparam>
        /// <param name="messageRecieved">The callback to be executed when a message is recieved</param>
        /// <param name="errorRecieved">The callback to be executed when an error occurs</param>
        /// <param name="channel">Used to override the MessageChannelAttribute from the class or to specify a channel to listen for messages on</param>
        /// <param name="group">Used to specify a group to associate to at the service layer (refer to groups in KubeMQ, Nats.IO, etc)</param>
        /// <param name="ignoreMessageHeader">If set to true this will cause the subscription to ignore the message type specified and assume that the type of message is of type T</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>An instance of the Subscription that can be held or called to end</returns>
        /// <exception cref="SubscriptionFailedException">An exception thrown when the subscription has failed to establish</exception>
        public ValueTask<ISubscription> SubscribeAsync<T>(Func<IRecievedMessage<T>, ValueTask> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = default) where T : class
            => CreateSubscriptionAsync<T>(messageRecieved, errorRecieved, channel, group, ignoreMessageHeader,false, cancellationToken);

        /// <summary>
        /// Called to establish a Subscription in the sevice layer for the Pub/Sub style messaging processing messages synchronously
        /// </summary>
        /// <typeparam name="T">The type of message to listen for</typeparam>
        /// <param name="messageRecieved">The callback to be executed when a message is recieved</param>
        /// <param name="errorRecieved">The callback to be executed when an error occurs</param>
        /// <param name="channel">Used to override the MessageChannelAttribute from the class or to specify a channel to listen for messages on</param>
        /// <param name="group">Used to specify a group to associate to at the service layer (refer to groups in KubeMQ, Nats.IO, etc)</param>
        /// <param name="ignoreMessageHeader">If set to true this will cause the subscription to ignore the message type specified and assume that the type of message is of type T</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>An instance of the Subscription that can be held or called to end</returns>
        /// <exception cref="SubscriptionFailedException">An exception thrown when the subscription has failed to establish</exception>
        public ValueTask<ISubscription> SubscribeAsync<T>(Action<IRecievedMessage<T>> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = default) where T : class
            => CreateSubscriptionAsync<T>((msg) =>
            {
                messageRecieved(msg);
                return ValueTask.CompletedTask;
            }, 
            errorRecieved, channel, group, ignoreMessageHeader, true, cancellationToken);

        private async ValueTask<ISubscription> CreateSubscriptionAsync<T>(Func<IRecievedMessage<T>, ValueTask> messageRecieved, Action<Exception> errorRecieved, string? channel, string? group, bool ignoreMessageHeader,bool synchronous, CancellationToken cancellationToken)
            where T : class
        {
            var subscription = new PubSubSubscription<T>(GetMessageFactory<T>(ignoreMessageHeader),
                messageRecieved,
                errorRecieved,
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
            var serviceMessage = await ProduceServiceMessage<Q>(ChannelMapper.MapTypes.Query, message, channel: channel, messageHeader: messageHeader);
            if (serviceConnection is IQueryableMessageServiceConnection queryableMessageServiceConnection) 
                return await ProduceResultAsync<R>(await queryableMessageServiceConnection.QueryAsync(
                    serviceMessage,
                    realTimeout??queryableMessageServiceConnection.DefaultTimout,
                    cancellationToken
                ));
            responseChannel ??=typeof(Q).GetCustomAttribute<QueryResponseChannelAttribute>()?.Name;
            ArgumentNullException.ThrowIfNullOrWhiteSpace(responseChannel);
            var replyChannel = await MapChannel(ChannelMapper.MapTypes.QueryResponse, responseChannel!);
            var callID = Guid.NewGuid();
            var (tcs,token) = await QueryResponseHelper.StartResponseListenerAsync(
                serviceConnection,
                realTimeout??TimeSpan.FromMinutes(1),
                Indentifier,
                callID,
                replyChannel,
                cancellationToken
            );
            var msg = QueryResponseHelper.EncodeMessage(
                serviceMessage,
                Indentifier,
                callID,
                replyChannel,
                null
            );
            await serviceConnection.PublishAsync(msg, cancellationToken: cancellationToken);
            try
            {
                await tcs.Task.WaitAsync(cancellationToken);
            }finally
            {
                if (!token.IsCancellationRequested)
                    await token.CancelAsync();
            }
            return await ProduceResultAsync<R>(tcs.Task.Result);
        }

        /// <summary>
        /// Called to publish a message in the Query/Response style
        /// </summary>
        /// <typeparam name="Q">The type of message to transmit for the Query</typeparam>
        /// <typeparam name="R">The type of message expected as a response</typeparam>
        /// <param name="message">The message to transmit for the query</param>
        /// <param name="timeout">The timeout to allow for waiting for a response</param>
        /// <param name="channel">Used to override the MessageChannelAttribute from the class or to specify a channel to transmit the message on</param>
        /// <param name="responseChannel">Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
        /// only used when the underlying connection does not support a QueryResponse style messaging.</param>
        /// <param name="messageHeader">A message header to be sent across with the message</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// 
        /// <returns>A QueryResult that will contain the response message and or an error</returns>
        public async ValueTask<QueryResult<R>> QueryAsync<Q, R>(Q message, TimeSpan? timeout = null, string? channel = null, string? responseChannel = null, MessageHeader? messageHeader = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class
            where R : class
            => await ExecuteQueryAsync<Q, R>(message, timeout: timeout, channel: channel,responseChannel:responseChannel, messageHeader: messageHeader, cancellationToken: cancellationToken);

        /// <summary>
        /// Called to publish a message in the Query/Response style except the response Type is gathered from the QueryResponseTypeAttribute
        /// </summary>
        /// <typeparam name="Q">The type of message to transmit for the Query</typeparam>
        /// <param name="message">The message to transmit for the query</param>
        /// <param name="timeout">The timeout to allow for waiting for a response</param>
        /// <param name="channel">Used to override the MessageChannelAttribute from the class or to specify a channel to transmit the message on</param>
        /// <param name="responseChannel">Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
        /// only used when the underlying connection does not support a QueryResponse style messaging.</param>
        /// <param name="messageHeader">A message header to be sent across with the message</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// 
        /// <returns>A QueryResult that will contain the response message and or an error</returns>
        /// <exception cref="UnknownResponseTypeException">Thrown when the supplied Query type does not have a QueryResponseTypeAttribute and therefore a response type cannot be determined</exception>
        public async ValueTask<QueryResult<object>> QueryAsync<Q>(Q message, TimeSpan? timeout = null, string? channel = null, string? responseChannel = null, MessageHeader? messageHeader = null,
            CancellationToken cancellationToken = default) where Q : class
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var responseType = (typeof(Q).GetCustomAttribute<QueryResponseTypeAttribute>(false)?.ResponseType)??throw new UnknownResponseTypeException("ResponseType", typeof(Q));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            var methodInfo = typeof(ContractConnection).GetMethod(nameof(ContractConnection.ExecuteQueryAsync), BindingFlags.NonPublic | BindingFlags.Instance)!.MakeGenericMethod(typeof(Q), responseType!);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            var queryResult = (dynamic?)await Utility.InvokeMethodAsync(
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
            return new QueryResult<object>(queryResult?.ID??string.Empty, queryResult?.Header??new MessageHeader([]), queryResult?.Result, queryResult?.Error);
        }

        private async ValueTask<QueryResult<R>> ProduceResultAsync<R>(ServiceQueryResult queryResult) where R : class
        {
            try
            {
                return new(
                    queryResult.ID,
                    queryResult.Header,
                    Result: await GetMessageFactory<R>().ConvertMessageAsync(logger, queryResult)
                );
            }catch(QueryResponseException qre)
            {
                return new(
                    queryResult.ID,
                    queryResult.Header,
                    Result: default,
                    Error: qre.Message
                );
            }
            catch(Exception ex)
            {
                return new(
                    queryResult.ID,
                    queryResult.Header,
                    Result: default,
                    Error: ex.Message
                );
            }
        }

        /// <summary>
        /// Creates a subscription with the underlying service layer for the Query/Response style processing messages asynchronously
        /// </summary>
        /// <typeparam name="Q">The expected message type for the Query</typeparam>
        /// <typeparam name="R">The expected message type for the Response</typeparam>
        /// <param name="messageRecieved">The callback to be executed when a message is recieved and expects a returned response</param>
        /// <param name="errorRecieved">The callback to be executed when an error occurs</param>
        /// <param name="channel">Used to override the MessageChannelAttribute from the class or to specify a channel to listen for messages on</param>
        /// <param name="group">Used to specify a group to associate to at the service layer (refer to groups in KubeMQ, Nats.IO, etc)</param>
        /// <param name="ignoreMessageHeader">If set to true this will cause the subscription to ignore the message type specified and assume that the type of message is of type T</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// 
        /// <returns>An instance of the Subscription that can be held or called to end</returns>
        /// <exception cref="SubscriptionFailedException">An exception thrown when the subscription has failed to establish</exception>
        public ValueTask<ISubscription> SubscribeQueryAsyncResponseAsync<Q, R>(Func<IRecievedMessage<Q>, ValueTask<QueryResponseMessage<R>>> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = default)
            where Q : class
            where R : class
        => ProduceSubscribeQueryResponseAsync<Q,R>(messageRecieved, errorRecieved, channel, group, ignoreMessageHeader,false, cancellationToken);

        /// <summary>
        /// Creates a subscription with the underlying service layer for the Query/Response style processing messages synchronously
        /// </summary>
        /// <typeparam name="Q">The expected message type for the Query</typeparam>
        /// <typeparam name="R">The expected message type for the Response</typeparam>
        /// <param name="messageRecieved">The callback to be executed when a message is recieved and expects a returned response</param>
        /// <param name="errorRecieved">The callback to be executed when an error occurs</param>
        /// <param name="channel">Used to override the MessageChannelAttribute from the class or to specify a channel to listen for messages on</param>
        /// <param name="group">Used to specify a group to associate to at the service layer (refer to groups in KubeMQ, Nats.IO, etc)</param>
        /// <param name="ignoreMessageHeader">If set to true this will cause the subscription to ignore the message type specified and assume that the type of message is of type T</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// 
        /// <returns>An instance of the Subscription that can be held or called to end</returns>
        /// <exception cref="SubscriptionFailedException">An exception thrown when the subscription has failed to establish</exception>
        public ValueTask<ISubscription> SubscribeQueryResponseAsync<Q, R>(Func<IRecievedMessage<Q>, QueryResponseMessage<R>> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = default)
            where Q : class
            where R : class
        => ProduceSubscribeQueryResponseAsync<Q, R>((msg) =>
            {
                var result = messageRecieved(msg);
                return ValueTask.FromResult(result);
            }, errorRecieved, channel, group, ignoreMessageHeader, true, cancellationToken);

        private async ValueTask<ISubscription> ProduceSubscribeQueryResponseAsync<Q, R>(Func<IRecievedMessage<Q>, ValueTask<QueryResponseMessage<R>>> messageRecieved, Action<Exception> errorRecieved, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, CancellationToken cancellationToken)
            where Q : class
            where R : class
        {
            var subscription = new QueryResponseSubscription<Q, R>(
                GetMessageFactory<Q>(ignoreMessageHeader),
                GetMessageFactory<R>(),
                messageRecieved,
                errorRecieved,
                (originalChannel) => MapChannel(ChannelMapper.MapTypes.QuerySubscription, originalChannel),
                channel: channel,
                group: group,
                synchronous: synchronous,
                logger: logger);
            if (await subscription.EstablishSubscriptionAsync(serviceConnection, cancellationToken))
                return subscription;
            throw new SubscriptionFailedException();
        }

        /// <summary>
        /// Called to close off this connection and it's underlying service connection
        /// </summary>
        /// <returns></returns>
        public ValueTask CloseAsync()
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

        /// <summary>
        /// Called to dispose of the resources contained within
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called to dispose of the resources contained within
        /// </summary>
        /// <returns>A task of the underlying resources being disposed</returns>
        public async ValueTask DisposeAsync()
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
