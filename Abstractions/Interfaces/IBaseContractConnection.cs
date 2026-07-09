using Microsoft.Extensions.Diagnostics.HealthChecks;
using MQContract.Messages;

namespace MQContract.Interfaces;

/// <summary>
/// Represents the Base for all Contract Connections and contains the definition of all items defined by all Contract Connections
/// </summary>
public interface IBaseContractConnection : IAsyncDisposable
{
    /// <summary>
    /// Called to create a subscription into the underlying service Pub/Sub style and have the messages processed asynchronously
    /// </summary>
    /// <example>
    /// <code>
    /// await contractConnection.SubscribeAsync&lt;ArrivalAnnouncement&gt;(
    ///     (message) => {
    ///         Console.WriteLine($"Arrival: {message.Message.FirstName}");
    ///         return ValueTask.CompletedTask;
    ///     },
    ///     (error) => Console.WriteLine($"Error: {error.Message}")
    /// );
    /// </code>
    /// </example>
    /// <typeparam name="TMessage">The type of message to listen for</typeparam>
    /// <param name="messageReceived">The callback invoked when a new message is received</param>
    /// <param name="errorReceived">The callback to invoke when an error occurs</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
    /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
    /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A subscription instance that can be ended when desired</returns>
    ValueTask<ISubscription> SubscribeAsync<TMessage>(Func<IReceivedMessage<TMessage>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel = null, string? group = null, bool ignoreMessageHeader = false,
        MessageFilters<TMessage>? messageFilters = null, CancellationToken cancellationToken = new CancellationToken());
    /// <summary>
    /// Called to create a subscription into the underlying service Pub/Sub style and have the messages processed syncrhonously
    /// </summary>
    /// <typeparam name="TMessage">The type of message to listen for</typeparam>
    /// <param name="messageReceived">The callback invoked when a new message is received</param>
    /// <param name="errorReceived">The callback to invoke when an error occurs</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
    /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
    /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// 
    /// 
    /// <returns>A subscription instance that can be ended when desired</returns>
    ValueTask<ISubscription> SubscribeAsync<TMessage>(Action<IReceivedMessage<TMessage>> messageReceived, Action<Exception> errorReceived, string? channel = null, string? group = null, bool ignoreMessageHeader = false,
        MessageFilters<TMessage>? messageFilters = null, CancellationToken cancellationToken = new CancellationToken());
    /// <summary>
    /// Called to create a subscription into the underlying service Query/Reponse style and have the messages processed asynchronously
    /// </summary>
    /// <typeparam name="TQuery">The type of message to listen for</typeparam>
    /// <typeparam name="TQueryResponse">The type of message to respond with</typeparam>
    /// <param name="messageReceived">The callback invoked when a new message is received expecting a response of the type response</param>
    /// <param name="errorReceived">The callback invoked when an error occurs.</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
    /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
    /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A subscription instance that can be ended when desired</returns>
    ValueTask<ISubscription> SubscribeQueryAsyncResponseAsync<TQuery, TQueryResponse>(Func<IReceivedMessage<TQuery>, ValueTask<QueryResponseMessage<TQueryResponse>>> messageReceived, Action<Exception> errorReceived, string? channel = null, string? group = null,
        bool ignoreMessageHeader = false, MessageFilters<TQuery>? messageFilters = null, CancellationToken cancellationToken = new CancellationToken());
    /// <summary>
    /// Called to create a subscription into the underlying service Query/Reponse style and have the messages processed synchronously
    /// </summary>
    /// <typeparam name="TQuery">The type of message to listen for</typeparam>
    /// <typeparam name="TQueryResponse">The type of message to respond with</typeparam>
    /// <param name="messageReceived">The callback invoked when a new message is received expecting a response of the type response</param>
    /// <param name="errorReceived">The callback invoked when an error occurs.</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class.</param>
    /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
    /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A subscription instance that can be ended when desired</returns>
    ValueTask<ISubscription> SubscribeQueryResponseAsync<TQuery, TQueryResponse>(Func<IReceivedMessage<TQuery>, QueryResponseMessage<TQueryResponse>> messageReceived, Action<Exception> errorReceived, string? channel = null, string? group = null,
        bool ignoreMessageHeader = false, MessageFilters<TQuery>? messageFilters = null, CancellationToken cancellationToken = new CancellationToken());
    /// <summary>
    /// Called to close off the contract connection and close it's underlying service connection
    /// </summary>
    /// <returns>A task for the closure of the connection</returns>
    ValueTask CloseAsync();
    /// <summary>
    /// Provides a usable HealthCheck implementation to provide HealthCheck information for the given Contract Connection
    /// </summary>
    IHealthCheck? HealthCheck { get; }
}
