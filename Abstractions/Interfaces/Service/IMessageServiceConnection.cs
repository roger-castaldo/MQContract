using MQContract.Messages;

namespace MQContract.Interfaces.Service
{
    /// <summary>
    /// Defines an underlying service connection.  This interface is used to allow for the creation of multiple underlying connection types to support the ability to use common code while
    /// being able to run against 1 or more Message services.
    /// </summary>
    public interface IMessageServiceConnection: IDisposable    
    {
        /// <summary>
        /// Maximum supported message body size in bytes
        /// </summary>
        int? MaxMessageBodySize { get; }
        /// <summary>
        /// The default timeout to use for RPC calls when it's not specified
        /// </summary>
        TimeSpan DefaultTimout { get; }
        /// <summary>
        /// Implemented Ping call if avaialble for the underlying service
        /// </summary>
        /// <returns>A Ping Result</returns>
        Task<PingResult> PingAsync();
        /// <summary>
        /// Implements a publish call to publish the given message
        /// </summary>
        /// <param name="message">The message to publish</param>
        /// <param name="options">The Service Channel Options instance that was supplied at the Contract Connection level</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A transmission result instance indicating the result</returns>
        Task<TransmissionResult> PublishAsync(ServiceMessage message, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken());
        /// <summary>
        /// Implements a call to create a subscription to a given channel as a member of a given group
        /// </summary>
        /// <param name="messageRecieved">The callback to invoke when a message is recieved</param>
        /// <param name="errorRecieved">The callback to invoke when an exception occurs</param>
        /// <param name="channel">The name of the channel to subscribe to</param>
        /// <param name="group">The subscription groupt to subscribe as</param>
        /// <param name="options">The Service Channel Options instance that was supplied at the Contract Connection level</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A service subscription object</returns>
        Task<IServiceSubscription?> SubscribeAsync(Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken());
        /// <summary>
        /// Implements a call to submit a response query request into the underlying service
        /// </summary>
        /// <param name="message">The message to query with</param>
        /// <param name="timeout">The timeout for recieving a response</param>
        /// <param name="options">The Service Channel Options instance that was supplied at the Contract Connection level</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A Query Result instance based on what happened</returns>
        Task<ServiceQueryResult> QueryAsync(ServiceMessage message, TimeSpan timeout, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken());
        /// <summary>
        /// Implements a call to create a subscription to a given channel as a member of a given group for responding to queries
        /// </summary>
        /// <param name="messageRecieved">The callback to be invoked when a message is recieved, returning the response message</param>
        /// <param name="errorRecieved">The callback to invoke when an exception occurs</param>
        /// <param name="channel">The name of the channel to subscribe to</param>
        /// <param name="group">The subscription groupt to subscribe as</param>
        /// <param name="options">The Service Channel Options instance that was supplied at the Contract Connection level</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A service subscription object</returns>
        Task<IServiceSubscription?> SubscribeQueryAsync(Func<RecievedServiceMessage, Task<ServiceMessage>> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken());
    }
}
