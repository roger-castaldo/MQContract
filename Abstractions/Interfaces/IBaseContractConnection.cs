using MQContract.Messages;

namespace MQContract.Interfaces
{
    public interface IBaseContractConnection : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Called to create a subscription into the underlying service Pub/Sub style and have the messages processed asynchronously
        /// </summary>
        /// <typeparam name="T">The type of message to listen for</typeparam>
        /// <param name="messageReceived">The callback invoked when a new message is received</param>
        /// <param name="errorReceived">The callback to invoke when an error occurs</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// 
        /// <returns>A subscription instance that can be ended when desired</returns>
        ValueTask<ISubscription> SubscribeAsync<T>(Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken())
            where T : class;
        /// <summary>
        /// Called to create a subscription into the underlying service Pub/Sub style and have the messages processed syncrhonously
        /// </summary>
        /// <typeparam name="T">The type of message to listen for</typeparam>
        /// <param name="messageReceived">The callback invoked when a new message is received</param>
        /// <param name="errorReceived">The callback to invoke when an error occurs</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// 
        /// <returns>A subscription instance that can be ended when desired</returns>
        ValueTask<ISubscription> SubscribeAsync<T>(Action<IReceivedMessage<T>> messageReceived, Action<Exception> errorReceived, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken())
            where T : class;
        /// <summary>
        /// Called to create a subscription into the underlying service Query/Reponse style and have the messages processed asynchronously
        /// </summary>
        /// <typeparam name="Q">The type of message to listen for</typeparam>
        /// <typeparam name="R">The type of message to respond with</typeparam>
        /// <param name="messageReceived">The callback invoked when a new message is received expecting a response of the type response</param>
        /// <param name="errorReceived">The callback invoked when an error occurs.</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// 
        /// <returns>A subscription instance that can be ended when desired</returns>
        ValueTask<ISubscription> SubscribeQueryAsyncResponseAsync<Q, R>(Func<IReceivedMessage<Q>, ValueTask<QueryResponseMessage<R>>> messageReceived, Action<Exception> errorReceived, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken())
            where Q : class
            where R : class;
        /// <summary>
        /// Called to create a subscription into the underlying service Query/Reponse style and have the messages processed synchronously
        /// </summary>
        /// <typeparam name="Q">The type of message to listen for</typeparam>
        /// <typeparam name="R">The type of message to respond with</typeparam>
        /// <param name="messageReceived">The callback invoked when a new message is received expecting a response of the type response</param>
        /// <param name="errorReceived">The callback invoked when an error occurs.</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// 
        /// <returns>A subscription instance that can be ended when desired</returns>
        ValueTask<ISubscription> SubscribeQueryResponseAsync<Q, R>(Func<IReceivedMessage<Q>, QueryResponseMessage<R>> messageReceived, Action<Exception> errorReceived, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken())
            where Q : class
            where R : class;
        /// <summary>
        /// Called to close off the contract connection and close it's underlying service connection
        /// </summary>
        /// <returns>A task for the closure of the connection</returns>
        ValueTask CloseAsync();
    }
}
