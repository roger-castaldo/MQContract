using MQContract.Messages;

namespace MQContract.Interfaces.Service
{
    /// <summary>
    /// Used to implement a service that supports bulk message publishing
    /// </summary>
    public interface IBulkPublishableMessageServiceConnection : IMessageServiceConnection
    {
        /// <summary>
        /// Implements a publish call to publish the given messages in bulk
        /// </summary>
        /// <param name="messages">The message to publish</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// 
        /// <returns>A transmission result instance indicating the result for each message</returns>
        ValueTask<IEnumerable<TransmissionResult>> BulkPublishAsync(IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken = new CancellationToken());
    }
}
