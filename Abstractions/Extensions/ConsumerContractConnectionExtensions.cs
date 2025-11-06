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
        /// <typeparam name="CC">Contract Connection</typeparam>
        /// <typeparam name="T">The Message type</typeparam>
        /// <typeparam name="TConsumer">The type that implements IPubSubConsumer&lt;T&gt;</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="consumer">An instance of the consumer</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<CC> RegisterPubSubConsumerAsync<CC, T, TConsumer>(
            this ValueTask<CC> connectionTask,
            TConsumer consumer,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            MessageFilters<T>? messageFilters = null,
            CancellationToken cancellationToken = default
        )
        where CC : class, IConsumerContractConnection<CC>
        where TConsumer : IPubSubConsumer<T>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterPubSubConsumerAsync<T, TConsumer>(
                consumer, channel, group, ignoreMessageHeader, messageFilters, cancellationToken
            ).ConfigureAwait(false);

            return connection;
        }
        /// <summary>
        /// Called to register a PubSubConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it. 
        /// </summary>
        /// <typeparam name="CC">Contract Connection</typeparam>
        /// <typeparam name="T">The Message type</typeparam>
        /// <typeparam name="TConsumer">The type that implements IPubSubConsumer&lt;T&gt;</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<CC> RegisterPubSubConsumerAsync<CC, T, TConsumer>(
            this ValueTask<CC> connectionTask,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            MessageFilters<T>? messageFilters = null,
            CancellationToken cancellationToken = default
        )
        where CC : class, IConsumerContractConnection<CC>
        where TConsumer : IPubSubConsumer<T>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterPubSubConsumerAsync<T, TConsumer>(
                channel, group, ignoreMessageHeader, messageFilters, cancellationToken
            ).ConfigureAwait(false);

            return connection;
        }
        /// <summary>
        /// Called to register a PubSubConsumer into the contract connection. 
        /// </summary>
        /// <typeparam name="CC">Contract Connection</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="consumerType">The type instance to be constructed and registered into the system.  It must implement IPubSubConsumer&lt;T&gt;.</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<CC> RegisterPubSubConsumerAsync<CC>(
            this ValueTask<CC> connectionTask,
            Type consumerType,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            CancellationToken cancellationToken = default
        )
        where CC : class, IConsumerContractConnection<CC>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterPubSubConsumerAsync(
                consumerType, channel, group, ignoreMessageHeader, cancellationToken
            ).ConfigureAwait(false);

            return connection;
        }
        #endregion

        #region PubSubAsyncConsumer
        /// <summary>
        /// Called to register a PubSubAsyncConsumer into the contract connection 
        /// </summary>
        /// <typeparam name="CC">Contract Connection</typeparam>
        /// <typeparam name="T">The Message type</typeparam>
        /// <typeparam name="TConsumer">The type that implements IPubSubAsyncConsumer&lt;T&gt;</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="consumer">An instance of the consumer</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<CC> RegisterPubSubAsyncConsumerAsync<CC, T, TConsumer>(
            this ValueTask<CC> connectionTask,
            TConsumer consumer,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            MessageFilters<T>? messageFilters = null,
            CancellationToken cancellationToken = default
        )
        where CC : class, IConsumerContractConnection<CC>
        where TConsumer : IPubSubAsyncConsumer<T>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterPubSubAsyncConsumerAsync<T, TConsumer>(
                consumer, channel, group, ignoreMessageHeader, messageFilters, cancellationToken
            ).ConfigureAwait(false);

            return connection;
        }
        /// <summary>
        /// Called to register a PubSubAsyncConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it. 
        /// </summary>
        /// <typeparam name="CC">Contract Connection</typeparam>
        /// <typeparam name="T">The Message type</typeparam>
        /// <typeparam name="TConsumer">The type that implements IPubSubAsyncConsumer&lt;T&gt;</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<CC> RegisterPubSubAsyncConsumerAsync<CC, T, TConsumer>(
            this ValueTask<CC> connectionTask,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            MessageFilters<T>? messageFilters = null,
            CancellationToken cancellationToken = default
        )
        where CC : class, IConsumerContractConnection<CC>
        where TConsumer : IPubSubAsyncConsumer<T>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterPubSubAsyncConsumerAsync<T, TConsumer>(
                channel, group, ignoreMessageHeader, messageFilters, cancellationToken
            ).ConfigureAwait(false);

            return connection;
        }
        /// <summary>
        /// Called to register a PubSubAsyncConsumer into the contract connection. 
        /// </summary>
        /// <typeparam name="CC">Contract Connection</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="consumerType">The type instance to be constructed and registered into the system.  It must implement IPubSubAsyncConsumer&lt;T&gt;.</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<CC> RegisterPubSubAsyncConsumerAsync<CC>(
            this ValueTask<CC> connectionTask,
            Type consumerType,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            CancellationToken cancellationToken = default
        )
        where CC : class, IConsumerContractConnection<CC>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterPubSubAsyncConsumerAsync(
                consumerType, channel, group, ignoreMessageHeader, cancellationToken
            ).ConfigureAwait(false);

            return connection;
        }
        #endregion

        #region QueryResponseConsumer
        /// <summary>
        /// Called to register a QueryResponseConsumer into the contract connection 
        /// </summary>
        /// <typeparam name="CC">Contract Connection</typeparam>
        /// <typeparam name="Q">The type of message to listen for</typeparam>
        /// <typeparam name="R">The type of message to respond with</typeparam>
        /// <typeparam name="TConsumer">The type that implements IQueryResponseConsumer&lt;Q,R&gt;</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="consumer">An instance of the consumer</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<CC> RegisterQueryResponseConsumerAsync<CC, Q, R, TConsumer>(
            this ValueTask<CC> connectionTask,
            TConsumer consumer,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            CancellationToken cancellationToken = default
        )
        where CC : class, IConsumerContractConnection<CC>
        where TConsumer : IQueryResponseConsumer<Q, R>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterQueryResponseConsumerAsync<Q, R, TConsumer>(
                consumer, channel, group, ignoreMessageHeader, cancellationToken
            ).ConfigureAwait(false);

            return connection;
        }
        /// <summary>
        /// Called to register a QueryResponseConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it. 
        /// </summary>
        /// <typeparam name="CC">Contract Connection</typeparam>
        /// <typeparam name="Q">The type of message to listen for</typeparam>
        /// <typeparam name="R">The type of message to respond with</typeparam>
        /// <typeparam name="TConsumer">The type that implements IQueryResponseConsumer&lt;Q,R&gt;</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<CC> RegisterQueryResponseConsumerAsync<CC, Q, R, TConsumer>(
            this ValueTask<CC> connectionTask,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            CancellationToken cancellationToken = default
        )
        where CC : class, IConsumerContractConnection<CC>
        where TConsumer : IQueryResponseConsumer<Q, R>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterQueryResponseConsumerAsync<Q, R, TConsumer>(
                channel, group, ignoreMessageHeader, cancellationToken
            ).ConfigureAwait(false);

            return connection;
        }
        /// <summary>
        /// Called to register a QueryResponseConsumer into the contract connection. 
        /// </summary>
        /// <typeparam name="CC">Contract Connection</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="consumerType">The type instance to be constructed and registered into the system.  It must implement IQueryResponseConsumer&lt;Q,R&gt;.</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<CC> RegisterQueryResponseConsumerAsync<CC>(
            this ValueTask<CC> connectionTask,
            Type consumerType,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            CancellationToken cancellationToken = default
        )
        where CC : class, IConsumerContractConnection<CC>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterQueryResponseConsumerAsync(
                consumerType, channel, group, ignoreMessageHeader, cancellationToken
            ).ConfigureAwait(false);

            return connection;
        }
        #endregion

        #region QueryResponseAsyncConsumer
        /// <summary>
        /// Called to register a QueryResponseAsyncConsumer into the contract connection 
        /// </summary>
        /// <typeparam name="CC">Contract Connection</typeparam>
        /// <typeparam name="Q">The type of message to listen for</typeparam>
        /// <typeparam name="R">The type of message to respond with</typeparam>
        /// <typeparam name="TConsumer">The type that implements IQueryResponseAsyncConsumer&lt;Q,R&gt;</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="consumer">An instance of the consumer</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<CC> RegisterQueryResponseAsyncConsumerAsync<CC, Q, R, TConsumer>(
            this ValueTask<CC> connectionTask,
            TConsumer consumer,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            CancellationToken cancellationToken = default
        )
        where CC : class, IConsumerContractConnection<CC>
        where TConsumer : IQueryResponseAsyncConsumer<Q, R>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterQueryResponseAsyncConsumerAsync<Q, R, TConsumer>(
                consumer, channel, group, ignoreMessageHeader, cancellationToken
            ).ConfigureAwait(false);

            return connection;
        }
        /// <summary>
        /// Called to register a QueryResponseAsyncConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it. 
        /// </summary>
        /// <typeparam name="CC">Contract Connection</typeparam>
        /// <typeparam name="Q">The type of message to listen for</typeparam>
        /// <typeparam name="R">The type of message to respond with</typeparam>
        /// <typeparam name="TConsumer">The type that implements IQueryResponseAsyncConsumer&lt;Q,R&gt;</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<CC> RegisterQueryResponseAsyncConsumerAsync<CC, Q, R, TConsumer>(
            this ValueTask<CC> connectionTask,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            CancellationToken cancellationToken = default
        )
        where CC : class, IConsumerContractConnection<CC>
        where TConsumer : IQueryResponseAsyncConsumer<Q, R>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterQueryResponseAsyncConsumerAsync<Q, R, TConsumer>(
                channel, group, ignoreMessageHeader, cancellationToken
            ).ConfigureAwait(false);

            return connection;
        }
        /// <summary>
        /// Called to register a QueryResponseAsyncConsumer into the contract connection. 
        /// </summary>
        /// <typeparam name="CC">Contract Connection</typeparam>
        /// <param name="connectionTask">Original Registration task</param>
        /// <param name="consumerType">The type instance to be constructed and registered into the system.  It must implement IQueryResponseAsyncConsumer&lt;Q,R&gt;.</param>
        /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
        /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
        /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        public static async ValueTask<CC> RegisterQueryResponseAsyncConsumerAsync<CC>(
            this ValueTask<CC> connectionTask,
            Type consumerType,
            string? channel = null,
            string? group = null,
            bool ignoreMessageHeader = false,
            CancellationToken cancellationToken = default
        )
        where CC : class, IConsumerContractConnection<CC>
        {
            var connection = await connectionTask.ConfigureAwait(false);
            await connection.RegisterQueryResponseAsyncConsumerAsync(
                consumerType, channel, group, ignoreMessageHeader, cancellationToken
            ).ConfigureAwait(false);

            return connection;
        }
        #endregion
    }
}
