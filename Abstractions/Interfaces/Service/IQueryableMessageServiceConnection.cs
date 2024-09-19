using MQContract.Messages;

namespace MQContract.Interfaces.Service
{
    /// <summary>
    /// Extends the base MessageServiceConnection Interface to Response Query messaging methodology if the underlying service supports it
    /// </summary>
    public interface IQueryableMessageServiceConnection : IMessageServiceConnection
    {
        /// <summary>
        /// The default timeout to use for RPC calls when it's not specified
        /// </summary>
        TimeSpan DefaultTimout { get; }
        /// <summary>
        /// Implements a call to submit a response query request into the underlying service
        /// </summary>
        /// <param name="message">The message to query with</param>
        /// <param name="timeout">The timeout for recieving a response</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A Query Result instance based on what happened</returns>
        ValueTask<ServiceQueryResult> QueryAsync(ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken = new CancellationToken());
        /// <summary>
        /// Implements a call to create a subscription to a given channel as a member of a given group for responding to queries
        /// </summary>
        /// <param name="messageReceived">The callback to be invoked when a message is received, returning the response message</param>
        /// <param name="errorReceived">The callback to invoke when an exception occurs</param>
        /// <param name="channel">The name of the channel to subscribe to</param>
        /// <param name="group">The group to bind a consumer to</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A service subscription object</returns>
        ValueTask<IServiceSubscription?> SubscribeQueryAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage>> messageReceived, Action<Exception> errorReceived, string channel, string? group = null, CancellationToken cancellationToken = new CancellationToken());
    }
}
