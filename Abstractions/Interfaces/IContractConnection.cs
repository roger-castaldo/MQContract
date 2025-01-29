using MQContract.Messages;

namespace MQContract.Interfaces
{
    /// <summary>
    /// This interface represents the Core class for the MQContract system, IE the ContractConnection
    /// </summary>
    public interface IContractConnection : IBaseContractConnection
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
        /// <param name="cancellationToken">A cancellation token</param>
        /// 
        /// <returns>A result indicating the tranmission results</returns>
        ValueTask<TransmissionResult> PublishAsync<T>(T message, string? channel = null, MessageHeader? messageHeader = null, CancellationToken cancellationToken = new CancellationToken());
        /// <summary>
        /// Called to send a bulk set of messages into the underlying service Pub/Sub style
        /// </summary>
        /// <typeparam name="T">The type of message to send</typeparam>
        /// <param name="messages">The set of messages to transmit, optionally with their given headers</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// 
        /// <returns>A result indicating the tranmission results</returns>
        ValueTask<IEnumerable<TransmissionResult>> BulkPublishAsync<T>(IEnumerable<(T message,MessageHeader? messageHeader)> messages, string? channel = null, CancellationToken cancellationToken = new CancellationToken());
        
        /// <summary>
        /// Called to send a message into the underlying service in the Query/Response style
        /// </summary>
        /// <typeparam name="Q">The type of message to send for the query</typeparam>
        /// <typeparam name="R">The type of message to expect back for the response</typeparam>
        /// <param name="message">The message to send</param>
        /// <param name="timeout">The allowed timeout prior to a response being received</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
        /// <param name="responseChannel">Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
        /// only used when the underlying connection does not support a QueryResponse style messaging.</param>
        /// <param name="messageHeader">The headers to pass along with the message</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// 
        /// <returns>A result indicating the success or failure as well as the returned message</returns>
        ValueTask<QueryResult<R>> QueryAsync<Q, R>(Q message, TimeSpan? timeout = null, string? channel = null, string? responseChannel = null, MessageHeader? messageHeader = null, CancellationToken cancellationToken = new CancellationToken());
        /// <summary>
        /// Called to send a message into the underlying service in the Query/Response style.  The return type is not specified here and is instead obtained from the QueryResponseTypeAttribute
        /// attached to the Query message type class.
        /// </summary>
        /// <typeparam name="Q">The type of message to send for the query</typeparam>
        /// <param name="message">The message to send</param>
        /// <param name="timeout">The allowed timeout prior to a response being received</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
        /// /// <param name="responseChannel">Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
        /// only used when the underlying connection does not support a QueryResponse style messaging.</param>
        /// <param name="messageHeader">The headers to pass along with the message</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// 
        /// <returns>A result indicating the success or failure as well as the returned message</returns>
        ValueTask<QueryResult<object>> QueryAsync<Q>(Q message, TimeSpan? timeout = null, string? channel = null, string? responseChannel = null, MessageHeader? messageHeader = null, CancellationToken cancellationToken = new CancellationToken());
    }
}
