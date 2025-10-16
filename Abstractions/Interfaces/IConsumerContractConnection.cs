using MQContract.Interfaces.Consumers;
using MQContract.Messages;
using System.Reflection;

namespace MQContract.Interfaces
{
    /// <summary>
    /// This interface represents a portion of the Contract Connection, specifically the portion for registering all Consumer classes
    /// </summary>
    public interface IConsumerContractConnection : IBaseContractConnection
    {
        /// <summary>
        /// Called to load all defined consumers found within the application, either within the supplied assembly or if null within the default LoadContext
        /// </summary>
        /// <param name="assembly">Optional parameter to specify loading from a single assembly, if not supplied will load all from within the default LoadContext</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A boolean indicating success or failure</returns>
        ValueTask<bool> AutoRegisterAllConsumersAsync(Assembly? assembly = null, CancellationToken cancellationToken = new CancellationToken());
        /// <summary>
        /// Called to register a PubSubConsumer into the contract connection 
        /// </summary>
        /// <typeparam name="T">The Message type</typeparam>
        /// <typeparam name="TConsumer">The type that implements IPubSubConsumer&lt;T&gt;</typeparam>
        /// <param name="consumer">An instance of the consumer</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
        /// <returns>A boolean indicating success or failure</returns>
        /// <param name="cancellationToken">A cancellation token</param>
        ValueTask<bool> RegisterPubSubConsumerAsync<T, TConsumer>(TConsumer consumer, string? channel = null, string? group = null, bool ignoreMessageHeader = false, MessageFilters<T>? messageFilters = null, CancellationToken cancellationToken = new CancellationToken())
            where TConsumer : IPubSubConsumer<T>;
        /// <summary>
        /// Called to register a PubSubConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it. 
        /// </summary>
        /// <typeparam name="T">The Message type</typeparam>
        /// <typeparam name="TConsumer">The type that implements IPubSubConsumer&lt;T&gt;</typeparam>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
        /// <returns>A boolean indicating success or failure</returns>
        /// <param name="cancellationToken">A cancellation token</param>
        ValueTask<bool> RegisterPubSubConsumerAsync<T, TConsumer>(string? channel = null, string? group = null, bool ignoreMessageHeader = false, MessageFilters<T>? messageFilters = null, CancellationToken cancellationToken = new CancellationToken())
            where TConsumer : IPubSubConsumer<T>;
        /// <summary>
        /// Called to register a PubSubConsumer into the contract connection. 
        /// </summary>
        /// <param name="consumerType">The type instance to be constructed and registered into the system.  It must implement IPubSubConsumer&lt;T&gt;.</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <returns>A boolean indicating success or failure</returns>
        /// <param name="cancellationToken">A cancellation token</param>
        ValueTask<bool> RegisterPubSubConsumerAsync(Type consumerType, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken());
        /// <summary>
        /// Called to register a PubSubAsyncConsumer into the contract connection 
        /// </summary>
        /// <typeparam name="T">The Message type</typeparam>
        /// <typeparam name="TConsumer">The type that implements PubSubAsyncConsumer&lt;T&gt;</typeparam>
        /// <param name="consumer">An instance of the consumer</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
        /// <returns>A boolean indicating success or failure</returns>
        /// <param name="cancellationToken">A cancellation token</param>
        ValueTask<bool> RegisterPubSubAsyncConsumerAsync<T, TConsumer>(TConsumer consumer, string? channel = null, string? group = null, bool ignoreMessageHeader = false, MessageFilters<T>? messageFilters = null, CancellationToken cancellationToken = new CancellationToken())
            where TConsumer : IPubSubAsyncConsumer<T>;
        /// <summary>
        /// Called to register a PubSubAsyncConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it. 
        /// </summary>
        /// <typeparam name="T">The Message type</typeparam>
        /// <typeparam name="TConsumer">The type that implements PubSubAsyncConsumer&lt;T&gt;</typeparam>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
        /// <returns>A boolean indicating success or failure</returns>
        /// <param name="cancellationToken">A cancellation token</param>
        ValueTask<bool> RegisterPubSubAsyncConsumerAsync<T, TConsumer>(string? channel = null, string? group = null, bool ignoreMessageHeader = false, MessageFilters<T>? messageFilters = null, CancellationToken cancellationToken = new CancellationToken())
            where TConsumer : IPubSubAsyncConsumer<T>;
        /// <summary>
        /// Called to register a PubSubAsyncConsumer into the contract connection. 
        /// </summary>
        /// <param name="consumerType">The type instance to be constructed and registered into the system.  It must implement IPubSubAsyncConsumer&lt;T&gt;.</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A boolean indicating success or failure</returns>
        /// 
        ValueTask<bool> RegisterPubSubAsyncConsumerAsync(Type consumerType, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken());
        /// <summary>
        /// Called to register a QueryResponseConsumer into the contract connection 
        /// </summary>
        /// <typeparam name="Q">The type of message to listen for</typeparam>
        /// <typeparam name="R">The type of message to respond with</typeparam>
        /// <typeparam name="TConsumer">The type that implements IQueryResponseConsumer&lt;Q,R&gt;</typeparam>
        /// <param name="consumer">An instance of the consumer</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A boolean indicating success or failure</returns>
        ValueTask<bool> RegisterQueryResponseConsumerAsync<Q, R, TConsumer>(TConsumer consumer, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken())
            where TConsumer : IQueryResponseConsumer<Q, R>;
        /// <summary>
        /// Called to register a QueryResponseConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it. 
        /// </summary>
        /// <typeparam name="Q">The type of message to listen for</typeparam>
        /// <typeparam name="R">The type of message to respond with</typeparam>
        /// <typeparam name="TConsumer">The type that implements IQueryResponseConsumer&lt;Q,R&gt;</typeparam>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A boolean indicating success or failure</returns>
        ValueTask<bool> RegisterQueryResponseConsumerAsync<Q, R, TConsumer>(string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken())
            where TConsumer : IQueryResponseConsumer<Q, R>;
        /// <summary>
        /// Called to register a QueryResponseConsumer into the contract connection. 
        /// </summary>
        /// <param name="consumerType">The type instance to be constructed and registered into the system.  It must implement IQueryResponseConsumer&lt;Q,R&gt;.</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A boolean indicating success or failure</returns>
        ValueTask<bool> RegisterQueryResponseConsumerAsync(Type consumerType, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken());
        /// <summary>
        /// Called to register a QueryResponseAsyncConsumer into the contract connection 
        /// </summary>
        /// <typeparam name="Q">The type of message to listen for</typeparam>
        /// <typeparam name="R">The type of message to respond with</typeparam>
        /// <typeparam name="TConsumer">The type that implements IQueryResponseAsyncConsumer&lt;Q,R&gt;</typeparam>
        /// <param name="consumer">An instance of the consumer</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A boolean indicating success or failure</returns>
        ValueTask<bool> RegisterQueryResponseAsyncConsumerAsync<Q, R, TConsumer>(TConsumer consumer, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken())
            where TConsumer : IQueryResponseAsyncConsumer<Q, R>;
        /// <summary>
        /// Called to register a QueryResponseAsyncConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it. 
        /// </summary>
        /// <typeparam name="Q">The type of message to listen for</typeparam>
        /// <typeparam name="R">The type of message to respond with</typeparam>
        /// <typeparam name="TConsumer">The type that implements IQueryResponseAsyncConsumer&lt;Q,R&gt;</typeparam>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A boolean indicating success or failure</returns>
        ValueTask<bool> RegisterQueryResponseAsyncConsumerAsync<Q, R, TConsumer>(string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken())
            where TConsumer : IQueryResponseAsyncConsumer<Q, R>;
        /// <summary>
        /// Called to register a QueryResponseAsyncConsumer into the contract connection. 
        /// </summary>
        /// <param name="consumerType">The type instance to be constructed and registered into the system.  It must implement IQueryResponseAsyncConsumer&lt;Q,R&gt;.</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A boolean indicating success or failure</returns>
        ValueTask<bool> RegisterQueryResponseAsyncConsumerAsync(Type consumerType, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken());
    }
}
