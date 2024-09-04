using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Interfaces
{
    /// <summary>
    /// This interface represents the Core class for the MQContract system, IE the ContractConnection
    /// </summary>
    public interface IContractConnection : IDisposable,IAsyncDisposable
    {
        /// <summary>
        /// Called to Ping the underlying system to obtain both information and ensure it is up.  Not all Services support this method.
        /// </summary>
        /// <returns></returns>
        ValueTask<PingResult> PingAsync();
        /// <summary>
        /// Called to send a message into the underlying service Pub/Sub style
        /// </summary>
        /// <typeparam name="T">The type of message to send</typeparam>
        /// <param name="message">The message to send</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
        /// <param name="messageHeader">The headers to pass along with the message</param>
        /// <param name="options">Any required Service Channel Options that will be passed down to the service Connection</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A result indicating the tranmission results</returns>
        ValueTask<TransmissionResult> PublishAsync<T>(T message, string? channel = null, MessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where T : class;
        /// <summary>
        /// Called to create a subscription into the underlying service Pub/Sub style and have the messages processed asynchronously
        /// </summary>
        /// <typeparam name="T">The type of message to listen for</typeparam>
        /// <param name="messageRecieved">The callback invoked when a new message is recieved</param>
        /// <param name="errorRecieved">The callback to invoke when an error occurs</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="options">Any required Service Channel Options that will be passed down to the service Connection</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A subscription instance that can be ended when desired</returns>
        ValueTask<ISubscription> SubscribeAsync<T>(Func<IRecievedMessage<T>,ValueTask> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, bool ignoreMessageHeader = false, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where T : class;
        /// <summary>
        /// Called to create a subscription into the underlying service Pub/Sub style and have the messages processed syncrhonously
        /// </summary>
        /// <typeparam name="T">The type of message to listen for</typeparam>
        /// <param name="messageRecieved">The callback invoked when a new message is recieved</param>
        /// <param name="errorRecieved">The callback to invoke when an error occurs</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="options">Any required Service Channel Options that will be passed down to the service Connection</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A subscription instance that can be ended when desired</returns>
        ValueTask<ISubscription> SubscribeAsync<T>(Action<IRecievedMessage<T>> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, bool ignoreMessageHeader = false, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where T : class;
        /// <summary>
        /// Called to send a message into the underlying service in the Query/Response style
        /// </summary>
        /// <typeparam name="Q">The type of message to send for the query</typeparam>
        /// <typeparam name="R">The type of message to expect back for the response</typeparam>
        /// <param name="message">The message to send</param>
        /// <param name="timeout">The allowed timeout prior to a response being recieved</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
        /// <param name="responseChannel">Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
        /// only used when the underlying connection does not support a QueryResponse style messaging.</param>
        /// <param name="messageHeader">The headers to pass along with the message</param>
        /// <param name="options">Any required Service Channel Options that will be passed down to the service Connection</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A result indicating the success or failure as well as the returned message</returns>
        ValueTask<QueryResult<R>> QueryAsync<Q, R>(Q message, TimeSpan? timeout = null, string? channel = null, string? responseChannel=null, MessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class
            where R : class;
        /// <summary>
        /// Called to send a message into the underlying service in the Query/Response style.  The return type is not specified here and is instead obtained from the QueryResponseTypeAttribute
        /// attached to the Query message type class.
        /// </summary>
        /// <typeparam name="Q">The type of message to send for the query</typeparam>
        /// <param name="message">The message to send</param>
        /// <param name="timeout">The allowed timeout prior to a response being recieved</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
        /// /// <param name="responseChannel">Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
        /// only used when the underlying connection does not support a QueryResponse style messaging.</param>
        /// <param name="messageHeader">The headers to pass along with the message</param>
        /// <param name="options">Any required Service Channel Options that will be passed down to the service Connection</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A result indicating the success or failure as well as the returned message</returns>
        ValueTask<QueryResult<object>> QueryAsync<Q>(Q message, TimeSpan? timeout = null, string? channel = null,string? responseChannel=null, MessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class;
        /// <summary>
        /// Called to create a subscription into the underlying service Query/Reponse style and have the messages processed asynchronously
        /// </summary>
        /// <typeparam name="Q">The type of message to listen for</typeparam>
        /// <typeparam name="R">The type of message to respond with</typeparam>
        /// <param name="messageRecieved">The callback invoked when a new message is recieved expecting a response of the type response</param>
        /// <param name="errorRecieved">The callback invoked when an error occurs.</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="options">Any required Service Channel Options that will be passed down to the service Connection</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A subscription instance that can be ended when desired</returns>
        ValueTask<ISubscription> SubscribeQueryAsyncResponseAsync<Q,R>(Func<IRecievedMessage<Q>,ValueTask<QueryResponseMessage<R>>> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, bool ignoreMessageHeader = false, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class
            where R : class;
        /// <summary>
        /// Called to create a subscription into the underlying service Query/Reponse style and have the messages processed synchronously
        /// </summary>
        /// <typeparam name="Q">The type of message to listen for</typeparam>
        /// <typeparam name="R">The type of message to respond with</typeparam>
        /// <param name="messageRecieved">The callback invoked when a new message is recieved expecting a response of the type response</param>
        /// <param name="errorRecieved">The callback invoked when an error occurs.</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="options">Any required Service Channel Options that will be passed down to the service Connection</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A subscription instance that can be ended when desired</returns>
        ValueTask<ISubscription> SubscribeQueryResponseAsync<Q, R>(Func<IRecievedMessage<Q>, QueryResponseMessage<R>> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, bool ignoreMessageHeader = false, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class
            where R : class;
        /// <summary>
        /// Called to close off the contract connection and close it's underlying service connection
        /// </summary>
        /// <returns>A task for the closure of the connection</returns>
        ValueTask CloseAsync();
    }
}
