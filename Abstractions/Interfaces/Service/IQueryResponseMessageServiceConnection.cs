using MQContract.Messages;

namespace MQContract.Interfaces.Service
{
    /// <summary>
    /// Extends the base MessageServiceConnection Interface to Response Query messaging methodology if the underlying service supports it
    /// </summary>
    public interface IQueryResponseMessageServiceConnection : IQueryableMessageServiceConnection
    {
        /// <summary>
        /// Implements a call to submit a response query request into the underlying service
        /// </summary>
        /// <param name="message">The message to query with</param>
        /// <param name="timeout">The timeout for recieving a response</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A Query Result instance based on what happened</returns>
        ValueTask<ServiceQueryResult> QueryAsync(ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken = new CancellationToken());
    }
}
