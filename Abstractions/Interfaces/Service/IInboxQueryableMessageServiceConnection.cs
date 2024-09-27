using MQContract.Messages;

namespace MQContract.Interfaces.Service
{
    /// <summary>
    /// Used to implement an Inbox style query response underlying service, this is if the service does not support QueryResponse messaging but does support a sort of query inbox response 
    /// style pub sub where you can specify the destination down to a specific instance.
    /// </summary>
    public interface IInboxQueryableMessageServiceConnection : IQueryableMessageServiceConnection
    {
        /// <summary>
        /// Establish the inbox subscription with the underlying service connection
        /// </summary>
        /// <param name="messageReceived">Callback called when a message is recieved in the RPC inbox</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A service subscription object specifically tied to the RPC inbox for this particular connection instance</returns>
        ValueTask<IServiceSubscription> EstablishInboxSubscriptionAsync(Action<ReceivedInboxServiceMessage> messageReceived,CancellationToken cancellationToken = new CancellationToken());
        /// <summary>
        /// Called to publish a Query Request when using the inbox style
        /// </summary>
        /// <param name="message">The service message to submit</param>
        /// <param name="correlationID">The unique ID of the message to use for handling when the response is proper and is expected in the inbox subscription</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The transmission result of submitting the message</returns>
        ValueTask<TransmissionResult> QueryAsync(ServiceMessage message,Guid correlationID, CancellationToken cancellationToken = new CancellationToken());
    }
}
