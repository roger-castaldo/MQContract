using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;
using MQContract.Messages;

namespace MQContract.Extensions
{
    /// <summary>
    /// Houses the extension calls to allow for fluent consumer registrations
    /// </summary>
    public static class ConsumerContractConnectionExtensions
    {
        #region PubSubConsumer
        /// <summary>
        /// Called to register a PubSubConsumer into the contract connection 
        /// </summary>
        /// <typeparam name="TContractConnection">Contract Connection</typeparam>
        /// <typeparam name="TMessage">The Message type</typeparam>
        /// <typeparam name="TConsumer">The type that implements IPubSubConsumer&lt;TMessage&gt;</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="consumer">An instance of the consumer</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<TContractConnection> RegisterPubSubConsumerAsync<TContractConnection, TMessage, TConsumer>(
            this ValueTask<IConsumerContractConnection<TContractConnection>> connectionTask,
            TConsumer consumer,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            MessageFilters<TMessage>? messageFilters = null,
            CancellationToken cancellationToken = default
        )
        where TContractConnection : IBaseContractConnection
        where TConsumer : IPubSubConsumer<TMessage>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterPubSubConsumerAsync<TMessage, TConsumer>(
                consumer, channel, group, ignoreMessageHeader, messageFilters, cancellationToken
            ).ConfigureAwait(false);

            return (TContractConnection)connection;
        }
        /// <summary>
        /// Called to register a PubSubConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it. 
        /// </summary>
        /// <typeparam name="TContractConnection">Contract Connection</typeparam>
        /// <typeparam name="TMessage">The Message type</typeparam>
        /// <typeparam name="TConsumer">The type that implements IPubSubConsumer&lt;TMessage&gt;</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<TContractConnection> RegisterPubSubConsumerAsync<TContractConnection, TMessage, TConsumer>(
            this ValueTask<IConsumerContractConnection<TContractConnection>> connectionTask,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            MessageFilters<TMessage>? messageFilters = null,
            CancellationToken cancellationToken = default
        )
        where TContractConnection : IBaseContractConnection
        where TConsumer : IPubSubConsumer<TMessage>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterPubSubConsumerAsync<TMessage, TConsumer>(
                channel, group, ignoreMessageHeader, messageFilters, cancellationToken
            ).ConfigureAwait(false);

            return (TContractConnection)connection;
        }
        /// <summary>
        /// Called to register a PubSubConsumer into the contract connection. 
        /// </summary>
        /// <typeparam name="TContractConnection">Contract Connection</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="consumerType">The type instance to be constructed and registered into the system.  It must implement IPubSubConsumer&lt;T&gt;.</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<TContractConnection> RegisterPubSubConsumerAsync<TContractConnection>(
            this ValueTask<IConsumerContractConnection<TContractConnection>> connectionTask,
            Type consumerType,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            CancellationToken cancellationToken = default
        )
        where TContractConnection : IBaseContractConnection
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterPubSubConsumerAsync(
                consumerType, channel, group, ignoreMessageHeader, cancellationToken
            ).ConfigureAwait(false);

            return (TContractConnection)connection;
        }
        #endregion

        #region PubSubAsyncConsumer
        /// <summary>
        /// Called to register a PubSubAsyncConsumer into the contract connection 
        /// </summary>
        /// <typeparam name="TContractConnection">Contract Connection</typeparam>
        /// <typeparam name="TMessage">The Message type</typeparam>
        /// <typeparam name="TConsumer">The type that implements IPubSubAsyncConsumer&lt;TMessage&gt;</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="consumer">An instance of the consumer</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<TContractConnection> RegisterPubSubAsyncConsumerAsync<TContractConnection, TMessage, TConsumer>(
            this ValueTask<IConsumerContractConnection<TContractConnection>> connectionTask,
            TConsumer consumer,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            MessageFilters<TMessage>? messageFilters = null,
            CancellationToken cancellationToken = default
        )
        where TContractConnection : IBaseContractConnection
        where TConsumer : IPubSubAsyncConsumer<TMessage>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterPubSubAsyncConsumerAsync<TMessage, TConsumer>(
                consumer, channel, group, ignoreMessageHeader, messageFilters, cancellationToken
            ).ConfigureAwait(false);

            return (TContractConnection)connection;
        }
        /// <summary>
        /// Called to register a PubSubAsyncConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it. 
        /// </summary>
        /// <typeparam name="TContractConnection">Contract Connection</typeparam>
        /// <typeparam name="TMessage">The Message type</typeparam>
        /// <typeparam name="TConsumer">The type that implements IPubSubAsyncConsumer&lt;TMessage&gt;</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<TContractConnection> RegisterPubSubAsyncConsumerAsync<TContractConnection, TMessage, TConsumer>(
            this ValueTask<IConsumerContractConnection<TContractConnection>> connectionTask,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            MessageFilters<TMessage>? messageFilters = null,
            CancellationToken cancellationToken = default
        )
        where TContractConnection : IBaseContractConnection
        where TConsumer : IPubSubAsyncConsumer<TMessage>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterPubSubAsyncConsumerAsync<TMessage, TConsumer>(
                channel, group, ignoreMessageHeader, messageFilters, cancellationToken
            ).ConfigureAwait(false);

            return (TContractConnection)connection;
        }
        /// <summary>
        /// Called to register a PubSubAsyncConsumer into the contract connection. 
        /// </summary>
        /// <typeparam name="TContractConnection">Contract Connection</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="consumerType">The type instance to be constructed and registered into the system.  It must implement IPubSubAsyncConsumer&lt;T&gt;.</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<TContractConnection> RegisterPubSubAsyncConsumerAsync<TContractConnection>(
            this ValueTask<IConsumerContractConnection<TContractConnection>> connectionTask,
            Type consumerType,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            CancellationToken cancellationToken = default
        )
        where TContractConnection : IBaseContractConnection
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterPubSubAsyncConsumerAsync(
                consumerType, channel, group, ignoreMessageHeader, cancellationToken
            ).ConfigureAwait(false);

            return (TContractConnection)connection;
        }
        #endregion

        #region QueryResponseConsumer
        /// <summary>
        /// Called to register a QueryResponseConsumer into the contract connection 
        /// </summary>
        /// <typeparam name="TContractConnection">Contract Connection</typeparam>
        /// <typeparam name="TQuery">The type of message to listen for</typeparam>
        /// <typeparam name="TQueryResponse">The type of message to respond with</typeparam>
        /// <typeparam name="TConsumer">The type that implements IQueryResponseConsumer&lt;TQuery,TQueryResponse&gt;</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="consumer">An instance of the consumer</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<TContractConnection> RegisterQueryResponseConsumerAsync<TContractConnection, TQuery, TQueryResponse, TConsumer>(
            this ValueTask<IConsumerContractConnection<TContractConnection>> connectionTask,
            TConsumer consumer,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            MessageFilters<TQuery>? messageFilters = null,
            CancellationToken cancellationToken = default
        )
        where TContractConnection : IBaseContractConnection
        where TConsumer : IQueryResponseConsumer<TQuery, TQueryResponse>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterQueryResponseConsumerAsync<TQuery, TQueryResponse, TConsumer>(
                consumer, channel, group, ignoreMessageHeader, messageFilters, cancellationToken
            ).ConfigureAwait(false);

            return (TContractConnection)connection;
        }
        /// <summary>
        /// Called to register a QueryResponseConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it. 
        /// </summary>
        /// <typeparam name="TContractConnection">Contract Connection</typeparam>
        /// <typeparam name="TQuery">The type of message to listen for</typeparam>
        /// <typeparam name="TQueryResponse">The type of message to respond with</typeparam>
        /// <typeparam name="TConsumer">The type that implements IQueryResponseConsumer&lt;TQuery,TQueryResponse&gt;</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<TContractConnection> RegisterQueryResponseConsumerAsync<TContractConnection, TQuery, TQueryResponse, TConsumer>(
            this ValueTask<IConsumerContractConnection<TContractConnection>> connectionTask,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            MessageFilters<TQuery>? messageFilters = null,
            CancellationToken cancellationToken = default
        )
        where TContractConnection : IBaseContractConnection
        where TConsumer : IQueryResponseConsumer<TQuery, TQueryResponse>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterQueryResponseConsumerAsync<TQuery, TQueryResponse, TConsumer>(
                channel, group, ignoreMessageHeader, messageFilters, cancellationToken
            ).ConfigureAwait(false);

            return (TContractConnection)connection;
        }
        /// <summary>
        /// Called to register a QueryResponseConsumer into the contract connection. 
        /// </summary>
        /// <typeparam name="TContractConnection">Contract Connection</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="consumerType">The type instance to be constructed and registered into the system.  It must implement IQueryResponseConsumer&lt;Q,R&gt;.</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<TContractConnection> RegisterQueryResponseConsumerAsync<TContractConnection>(
            this ValueTask<IConsumerContractConnection<TContractConnection>> connectionTask,
            Type consumerType,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            CancellationToken cancellationToken = default
        )
        where TContractConnection : IBaseContractConnection
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterQueryResponseConsumerAsync(
                consumerType, channel, group, ignoreMessageHeader, cancellationToken
            ).ConfigureAwait(false);

            return (TContractConnection)connection;
        }
        #endregion

        #region QueryResponseAsyncConsumer
        /// <summary>
        /// Called to register a QueryResponseAsyncConsumer into the contract connection 
        /// </summary>
        /// <typeparam name="TContractConnection">Contract Connection</typeparam>
        /// <typeparam name="TQuery">The type of message to listen for</typeparam>
        /// <typeparam name="TQueryResponse">The type of message to respond with</typeparam>
        /// <typeparam name="TConsumer">The type that implements IQueryResponseAsyncConsumer&lt;TQuery,TQueryResponse&gt;</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="consumer">An instance of the consumer</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<TContractConnection> RegisterQueryResponseAsyncConsumerAsync<TContractConnection, TQuery, TQueryResponse, TConsumer>(
            this ValueTask<IConsumerContractConnection<TContractConnection>> connectionTask,
            TConsumer consumer,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            MessageFilters<TQuery>? messageFilters = null,
            CancellationToken cancellationToken = default
        )
        where TContractConnection : IBaseContractConnection
        where TConsumer : IQueryResponseAsyncConsumer<TQuery, TQueryResponse>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterQueryResponseAsyncConsumerAsync<TQuery, TQueryResponse, TConsumer>(
                consumer, channel, group, ignoreMessageHeader, messageFilters, cancellationToken
            ).ConfigureAwait(false);

            return (TContractConnection)connection;
        }
        /// <summary>
        /// Called to register a QueryResponseAsyncConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it. 
        /// </summary>
        /// <typeparam name="TContractConnection">Contract Connection</typeparam>
        /// <typeparam name="TQuery">The type of message to listen for</typeparam>
        /// <typeparam name="TQueryResponse">The type of message to respond with</typeparam>
        /// <typeparam name="TConsumer">The type that implements IQueryResponseAsyncConsumer&lt;TQuery,TQueryResponse&gt;</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<TContractConnection> RegisterQueryResponseAsyncConsumerAsync<TContractConnection, TQuery, TQueryResponse, TConsumer>(
            this ValueTask<IConsumerContractConnection<TContractConnection>> connectionTask,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            MessageFilters<TQuery>? messageFilters = null,
            CancellationToken cancellationToken = default
        )
        where TContractConnection : IBaseContractConnection
        where TConsumer : IQueryResponseAsyncConsumer<TQuery, TQueryResponse>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterQueryResponseAsyncConsumerAsync<TQuery, TQueryResponse, TConsumer>(
                channel, group, ignoreMessageHeader, messageFilters, cancellationToken
            ).ConfigureAwait(false);

            return (TContractConnection)connection;
        }
        /// <summary>
        /// Called to register a QueryResponseAsyncConsumer into the contract connection. 
        /// </summary>
        /// <typeparam name="TContractConnection">Contract Connection</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="consumerType">The type instance to be constructed and registered into the system.  It must implement IQueryResponseAsyncConsumer&lt;Q,R&gt;.</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<TContractConnection> RegisterQueryResponseAsyncConsumerAsync<TContractConnection>(
            this ValueTask<IConsumerContractConnection<TContractConnection>> connectionTask,
            Type consumerType,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            CancellationToken cancellationToken = default
        )
        where TContractConnection : IBaseContractConnection
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterQueryResponseAsyncConsumerAsync(
                consumerType, channel, group, ignoreMessageHeader, cancellationToken
            ).ConfigureAwait(false);

            return (TContractConnection)connection;
        }
        #endregion
    }
}
