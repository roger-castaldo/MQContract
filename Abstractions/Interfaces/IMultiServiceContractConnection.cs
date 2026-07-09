using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Interfaces;

/// <summary>
/// This interface represents an extended Contract Connection that is built around the idea of having more than 1 underlying service connection that can be interacted with 
/// depending on the defined conditions
/// </summary>
public interface IMultiServiceContractConnection :
    IMappableContractConnection<IMultiServiceContractConnection>, IMetricContractConnection<IMultiServiceContractConnection>
{
    /// <summary>
    /// Register a service connection that will map to all calls
    /// </summary>
    /// <param name="serviceConnectionName">The name of the service connection, not necessarily unique, but can be used for logging and other things</param>
    /// <param name="messageServiceConnection">The service connection to use for all calls</param>
    /// <returns></returns>
    IMultiServiceContractConnection RegisterServiceConnection(string serviceConnectionName, IMessageServiceConnection messageServiceConnection);
    /// <summary>
    /// Called to Ping the underlying systems (assuming they implement the call) to obtain both information and ensure it is up.  Not all Services support this method.
    /// </summary>
    /// <returns></returns>
    ValueTask<IEnumerable<PingResult>> PingAsync();
    /// <summary>
    /// Called to send a message into the underlying services Pub/Sub style
    /// </summary>
    /// <typeparam name="TMessage">The type of message to send</typeparam>
    /// <param name="message">The message to send</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
    /// <param name="messageHeader">The headers to pass along with the message</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A result indicating the tranmission results</returns>
    [Obsolete("This method is obsolete and will be removed in the next update, please change to the method using TransmissionMessage or the simplified call with no messageHeader capabilites")]
    ValueTask<MultiTransmissionResult> PublishAsync<TMessage>(TMessage message, string? channel = null, MessageHeader? messageHeader = null, CancellationToken cancellationToken = new CancellationToken());
    /// <summary>
    /// Called to send a message into the underlying services Pub/Sub style
    /// </summary>
    /// <typeparam name="TMessage">The type of message to send</typeparam>
    /// <param name="message">The message to send</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A result indicating the tranmission results</returns>
    ValueTask<MultiTransmissionResult> PublishAsync<TMessage>(TMessage message, string? channel = null, CancellationToken cancellationToken = new CancellationToken());
    /// <summary>
    /// Called to send a message into the underlying services Pub/Sub style
    /// </summary>
    /// <typeparam name="TMessage">The type of message to send</typeparam>
    /// <param name="message">The message to send with additional properties such as ID or Headers</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A result indicating the tranmission results</returns>
    ValueTask<MultiTransmissionResult> PublishAsync<TMessage>(TransmissionMessage<TMessage> message, string? channel = null, CancellationToken cancellationToken = new CancellationToken());
    /// <summary>
    /// Called to send a bulk set of messages into the underlying services Pub/Sub style
    /// </summary>
    /// <typeparam name="TMessage">The type of message to send</typeparam>
    /// <param name="messages">The set of messages to transmit, optionally with their given headers</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A result indicating the tranmission results</returns>
    [Obsolete("This method is obsolete and will be removed in the next update, please change to the method using TransmissionMessage or the simplified call with no messageHeader capabilites")]
    ValueTask<IEnumerable<MultiTransmissionResult>> BulkPublishAsync<TMessage>(IEnumerable<(TMessage message, MessageHeader? messageHeader)> messages, string? channel = null, CancellationToken cancellationToken = new CancellationToken());
    /// <summary>
    /// Called to send a bulk set of messages into the underlying services Pub/Sub style
    /// </summary>
    /// <typeparam name="TMessage">The type of message to send</typeparam>
    /// <param name="messages">The set of messages to transmit</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A result indicating the tranmission results</returns>
    ValueTask<IEnumerable<MultiTransmissionResult>> BulkPublishAsync<TMessage>(IEnumerable<TMessage> messages, string? channel = null, CancellationToken cancellationToken = new CancellationToken());
    /// <summary>
    /// Called to send a bulk set of messages into the underlying services Pub/Sub style
    /// </summary>
    /// <typeparam name="TMessage">The type of message to send</typeparam>
    /// <param name="messages">The set of messages to transmit with additional properties such as ID or Headers</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A result indicating the tranmission results</returns>
    ValueTask<IEnumerable<MultiTransmissionResult>> BulkPublishAsync<TMessage>(IEnumerable<TransmissionMessage<TMessage>> messages, string? channel = null, CancellationToken cancellationToken = new CancellationToken());
    /// <summary>
    /// Called to send a message into the underlying services in the Query/Response style
    /// </summary>
    /// <typeparam name="TQuery">The type of message to send for the query</typeparam>
    /// <typeparam name="TQueryResponse">The type of message to expect back for the response</typeparam>
    /// <param name="message">The message to send</param>
    /// <param name="timeout">The allowed timeout prior to a response being received</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
    /// <param name="responseChannel">Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
    /// only used when the underlying connection does not support a QueryResponse style messaging.</param>
    /// <param name="messageHeader">The headers to pass along with the message</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A result indicating the success or failure as well as the returned message</returns>
    [Obsolete("This method is obsolete and will be removed in the next update, please change to the method using TransmissionMessage or the simplified call with no messageHeader capabilites")]
    ValueTask<IEnumerable<QueryResult<TQueryResponse>>> QueryAsync<TQuery, TQueryResponse>(TQuery message, TimeSpan? timeout = null, string? channel = null, string? responseChannel = null, MessageHeader? messageHeader = null, CancellationToken cancellationToken = new CancellationToken());
    /// <summary>
    /// Called to send a message into the underlying services in the Query/Response style
    /// </summary>
    /// <typeparam name="TQuery">The type of message to send for the query</typeparam>
    /// <typeparam name="TQueryResponse">The type of message to expect back for the response</typeparam>
    /// <param name="message">The message to send</param>
    /// <param name="timeout">The allowed timeout prior to a response being received</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
    /// <param name="responseChannel">Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
    /// only used when the underlying connection does not support a QueryResponse style messaging.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A result indicating the success or failure as well as the returned message</returns>
    ValueTask<IEnumerable<QueryResult<TQueryResponse>>> QueryAsync<TQuery, TQueryResponse>(TQuery message, TimeSpan? timeout = null, string? channel = null, string? responseChannel = null, CancellationToken cancellationToken = new CancellationToken());
    /// <summary>
    /// Called to send a message into the underlying services in the Query/Response style
    /// </summary>
    /// <typeparam name="TQuery">The type of message to send for the query</typeparam>
    /// <typeparam name="TQueryResponse">The type of message to expect back for the response</typeparam>
    /// <param name="message">The message to send with additional properties such as ID or Headers</param>
    /// <param name="timeout">The allowed timeout prior to a response being received</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
    /// <param name="responseChannel">Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
    /// only used when the underlying connection does not support a QueryResponse style messaging.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A result indicating the success or failure as well as the returned message</returns>
    ValueTask<IEnumerable<QueryResult<TQueryResponse>>> QueryAsync<TQuery, TQueryResponse>(TransmissionMessage<TQuery> message, TimeSpan? timeout = null, string? channel = null, string? responseChannel = null, CancellationToken cancellationToken = new CancellationToken());
    /// <summary>
    /// Called to send a message into the underlying services in the Query/Response style.  The return type is not specified here and is instead obtained from the QueryResponseTypeAttribute
    /// attached to the Query message type class.
    /// </summary>
    /// <typeparam name="TQuery">The type of message to send for the query</typeparam>
    /// <param name="message">The message to send</param>
    /// <param name="timeout">The allowed timeout prior to a response being received</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
    /// /// <param name="responseChannel">Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
    /// only used when the underlying connection does not support a QueryResponse style messaging.</param>
    /// <param name="messageHeader">The headers to pass along with the message</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A result indicating the success or failure as well as the returned message</returns>
    [Obsolete("This method is obsolete and will be removed in the next update, please change to the method using TransmissionMessage or the simplified call with no messageHeader capabilites")]
    ValueTask<IEnumerable<QueryResult<object>>> QueryAsync<TQuery>(TQuery message, TimeSpan? timeout = null, string? channel = null, string? responseChannel = null, MessageHeader? messageHeader = null, CancellationToken cancellationToken = new CancellationToken());
    /// <summary>
    /// Called to send a message into the underlying services in the Query/Response style.  The return type is not specified here and is instead obtained from the QueryResponseTypeAttribute
    /// attached to the Query message type class.
    /// </summary>
    /// <typeparam name="TQuery">The type of message to send for the query</typeparam>
    /// <param name="message">The message to send</param>
    /// <param name="timeout">The allowed timeout prior to a response being received</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
    /// /// <param name="responseChannel">Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
    /// only used when the underlying connection does not support a QueryResponse style messaging.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A result indicating the success or failure as well as the returned message</returns>
    ValueTask<IEnumerable<QueryResult<object>>> QueryAsync<TQuery>(TQuery message, TimeSpan? timeout = null, string? channel = null, string? responseChannel = null, CancellationToken cancellationToken = new CancellationToken());
    /// <summary>
    /// Called to send a message into the underlying services in the Query/Response style.  The return type is not specified here and is instead obtained from the QueryResponseTypeAttribute
    /// attached to the Query message type class.
    /// </summary>
    /// <typeparam name="TQuery">The type of message to send for the query</typeparam>
    /// <param name="message">The message to send with additional properties such as ID or Headers</param>
    /// <param name="timeout">The allowed timeout prior to a response being received</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
    /// /// <param name="responseChannel">Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
    /// only used when the underlying connection does not support a QueryResponse style messaging.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A result indicating the success or failure as well as the returned message</returns>
    ValueTask<IEnumerable<QueryResult<object>>> QueryAsync<TQuery>(TransmissionMessage<TQuery> message, TimeSpan? timeout = null, string? channel = null, string? responseChannel = null, CancellationToken cancellationToken = new CancellationToken());
}
