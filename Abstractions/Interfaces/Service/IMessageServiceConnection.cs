using MQContract.Messages;

namespace MQContract.Interfaces.Service
{
    /// <summary>
    /// Defines an underlying service connection.  This interface is used to allow for the creation of multiple underlying connection types to support the ability to use common code while
    /// being able to run against 1 or more Message services.
    /// </summary>
    public interface IMessageServiceConnection
    {
        /// <summary>
        /// Maximum supported message body size in bytes
        /// </summary>
        uint? MaxMessageBodySize { get; }
        /// <summary>
        /// Implements a publish call to publish the given message
        /// </summary>
        /// <param name="message">The message to publish</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// 
        /// <returns>A transmission result instance indicating the result</returns>
        ValueTask<TransmissionResult> PublishAsync(ServiceMessage message, CancellationToken cancellationToken = new CancellationToken());
        /// <summary>
        /// Implements a call to create a subscription to a given channel as a member of a given group
        /// </summary>
        /// <param name="messageReceived">The callback to invoke when a message is received</param>
        /// <param name="errorReceived">The callback to invoke when an exception occurs</param>
        /// <param name="channel">The name of the channel to subscribe to</param>
        /// <param name="group">The consumer group to register as</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A service subscription object</returns>
        ValueTask<IServiceSubscription?> SubscribeAsync(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel, string? group = null, CancellationToken cancellationToken = new CancellationToken());
        /// <summary>
        /// Implements a call to close off the connection when the ContractConnection is closed
        /// </summary>
        /// <returns>A task that the close is running in</returns>
        ValueTask CloseAsync();
    }
}
