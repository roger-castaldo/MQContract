
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;
using MQContract.Interfaces.Middleware;
using MQContract.Messages;

namespace MQContract.Extensions;

/// <summary>
/// Houses the extension calls to allow for fluent MultiService Connection calls
/// </summary>
public static class MultiServiceConnectionExtensions
{
    #region PubSubConsumer
    /// <summary>
    /// Called to register a PubSubConsumer into the contract connection 
    /// </summary>
    /// <typeparam name="TMessage">The Message type</typeparam>
    /// <typeparam name="TConsumer">The type that implements IPubSubConsumer&lt;TMessage&gt;</typeparam>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="consumer">An instance of the consumer</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
    /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
    /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterPubSubConsumerAsync<TMessage, TConsumer>(
        this ValueTask<IMultiServiceContractConnection> connectionTask,
        TConsumer consumer,
        string? channel = null,
        string? group = null,
        bool ignoreMessageHeader = false,
        MessageFilters<TMessage>? messageFilters = null,
        CancellationToken cancellationToken = default
    )
    where TConsumer : IPubSubConsumer<TMessage>
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterPubSubConsumerAsync<TMessage, TConsumer>(
            consumer, channel, group, ignoreMessageHeader, messageFilters, cancellationToken
        ).ConfigureAwait(false);

        return connection;
    }
    /// <summary>
    /// Called to register a PubSubConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it. 
    /// </summary>
    /// <typeparam name="TMessage">The Message type</typeparam>
    /// <typeparam name="TConsumer">The type that implements IPubSubConsumer&lt;TMessage&gt;</typeparam>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
    /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
    /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterPubSubConsumerAsync<TMessage, TConsumer>(
        this ValueTask<IMultiServiceContractConnection> connectionTask,
        string? channel = null,
        string? group = null,
        bool ignoreMessageHeader = false,
        MessageFilters<TMessage>? messageFilters = null,
        CancellationToken cancellationToken = default
    )
    where TConsumer : IPubSubConsumer<TMessage>
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterPubSubConsumerAsync<TMessage, TConsumer>(
            channel, group, ignoreMessageHeader, messageFilters, cancellationToken
        ).ConfigureAwait(false);

        return connection;
    }
    /// <summary>
    /// Called to register a PubSubConsumer into the contract connection. 
    /// </summary>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="consumerType">The type instance to be constructed and registered into the system.  It must implement IPubSubConsumer&lt;T&gt;.</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
    /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
    /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterPubSubConsumerAsync(
        this ValueTask<IMultiServiceContractConnection> connectionTask,
        Type consumerType,
        string? channel = null,
        string? group = null,
        bool ignoreMessageHeader = false,
        CancellationToken cancellationToken = default
    )
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
    /// <typeparam name="TMessage">The Message type</typeparam>
    /// <typeparam name="TConsumer">The type that implements IPubSubAsyncConsumer&lt;TMessage&gt;</typeparam>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="consumer">An instance of the consumer</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
    /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
    /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterPubSubAsyncConsumerAsync<TMessage, TConsumer>(
        this ValueTask<IMultiServiceContractConnection> connectionTask,
        TConsumer consumer,
        string? channel = null,
        string? group = null,
        bool ignoreMessageHeader = false,
        MessageFilters<TMessage>? messageFilters = null,
        CancellationToken cancellationToken = default
    )
    where TConsumer : IPubSubAsyncConsumer<TMessage>
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterPubSubAsyncConsumerAsync<TMessage, TConsumer>(
            consumer, channel, group, ignoreMessageHeader, messageFilters, cancellationToken
        ).ConfigureAwait(false);

        return connection;
    }
    /// <summary>
    /// Called to register a PubSubAsyncConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it. 
    /// </summary>
    /// <typeparam name="TMessage">The Message type</typeparam>
    /// <typeparam name="TConsumer">The type that implements IPubSubAsyncConsumer&lt;TMessage&gt;</typeparam>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
    /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
    /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterPubSubAsyncConsumerAsync<TMessage, TConsumer>(
        this ValueTask<IMultiServiceContractConnection> connectionTask,
        string? channel = null,
        string? group = null,
        bool ignoreMessageHeader = false,
        MessageFilters<TMessage>? messageFilters = null,
        CancellationToken cancellationToken = default
    )
    where TConsumer : IPubSubAsyncConsumer<TMessage>
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterPubSubAsyncConsumerAsync<TMessage, TConsumer>(
            channel, group, ignoreMessageHeader, messageFilters, cancellationToken
        ).ConfigureAwait(false);

        return connection;
    }
    /// <summary>
    /// Called to register a PubSubAsyncConsumer into the contract connection. 
    /// </summary>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="consumerType">The type instance to be constructed and registered into the system.  It must implement IPubSubAsyncConsumer&lt;T&gt;.</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
    /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
    /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterPubSubAsyncConsumerAsync(
        this ValueTask<IMultiServiceContractConnection> connectionTask,
        Type consumerType,
        string? channel = null,
        string? group = null,
        bool ignoreMessageHeader = false,
        CancellationToken cancellationToken = default
    )
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
    /// <typeparam name="TQuery">The type of message to listen for</typeparam>
    /// <typeparam name="TQueryResponse">The type of message to respond with</typeparam>
    /// <typeparam name="TConsumer">The type that implements IQueryResponseConsumer&lt;TQuery,TQueryResponse&gt;</typeparam>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="consumer">An instance of the consumer</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
    /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
    /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterQueryResponseConsumerAsync<TQuery, TQueryResponse, TConsumer>(
        this ValueTask<IMultiServiceContractConnection> connectionTask,
        TConsumer consumer,
        string? channel = null,
        string? group = null,
        bool ignoreMessageHeader = false,
        MessageFilters<TQuery>? messageFilters = null,
        CancellationToken cancellationToken = default
    )
    where TConsumer : IQueryResponseConsumer<TQuery, TQueryResponse>
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterQueryResponseConsumerAsync<TQuery, TQueryResponse, TConsumer>(
            consumer, channel, group, ignoreMessageHeader, messageFilters, cancellationToken
        ).ConfigureAwait(false);

        return connection;
    }
    /// <summary>
    /// Called to register a QueryResponseConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it. 
    /// </summary>
    /// <typeparam name="TQuery">The type of message to listen for</typeparam>
    /// <typeparam name="TQueryResponse">The type of message to respond with</typeparam>
    /// <typeparam name="TConsumer">The type that implements IQueryResponseConsumer&lt;TQuery,TQueryResponse&gt;</typeparam>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
    /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
    /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterQueryResponseConsumerAsync<TQuery, TQueryResponse, TConsumer>(
        this ValueTask<IMultiServiceContractConnection> connectionTask,
        string? channel = null,
        string? group = null,
        bool ignoreMessageHeader = false,
        MessageFilters<TQuery>? messageFilters = null,
        CancellationToken cancellationToken = default
    )
    where TConsumer : IQueryResponseConsumer<TQuery, TQueryResponse>
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterQueryResponseConsumerAsync<TQuery, TQueryResponse, TConsumer>(
            channel, group, ignoreMessageHeader, messageFilters, cancellationToken
        ).ConfigureAwait(false);

        return connection;
    }
    /// <summary>
    /// Called to register a QueryResponseConsumer into the contract connection. 
    /// </summary>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="consumerType">The type instance to be constructed and registered into the system.  It must implement IQueryResponseConsumer&lt;Q,R&gt;.</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
    /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
    /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterQueryResponseConsumerAsync(
        this ValueTask<IMultiServiceContractConnection> connectionTask,
        Type consumerType,
        string? channel = null,
        string? group = null,
        bool ignoreMessageHeader = false,
        CancellationToken cancellationToken = default
    )
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
    /// <typeparam name="TQuery">The type of message to listen for</typeparam>
    /// <typeparam name="TQueryResponse">The type of message to respond with</typeparam>
    /// <typeparam name="TConsumer">The type that implements IQueryResponseAsyncConsumer&lt;TQuery,TQueryResponse&gt;</typeparam>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="consumer">An instance of the consumer</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
    /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
    /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterQueryResponseAsyncConsumerAsync<TQuery, TQueryResponse, TConsumer>(
        this ValueTask<IMultiServiceContractConnection> connectionTask,
        TConsumer consumer,
        string? channel = null,
        string? group = null,
        bool ignoreMessageHeader = false,
        MessageFilters<TQuery>? messageFilters = null,
        CancellationToken cancellationToken = default
    )
    where TConsumer : IQueryResponseAsyncConsumer<TQuery, TQueryResponse>
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterQueryResponseAsyncConsumerAsync<TQuery, TQueryResponse, TConsumer>(
            consumer, channel, group, ignoreMessageHeader, messageFilters, cancellationToken
        ).ConfigureAwait(false);

        return connection;
    }
    /// <summary>
    /// Called to register a QueryResponseAsyncConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it. 
    /// </summary>
    /// <typeparam name="TQuery">The type of message to listen for</typeparam>
    /// <typeparam name="TQueryResponse">The type of message to respond with</typeparam>
    /// <typeparam name="TConsumer">The type that implements IQueryResponseAsyncConsumer&lt;TQuery,TQueryResponse&gt;</typeparam>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
    /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
    /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    /// <param name="messageFilters">Provides any filtering options for this subscription to filter out messages prior to action calls if desired</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterQueryResponseAsyncConsumerAsync<TQuery, TQueryResponse, TConsumer>(
        this ValueTask<IMultiServiceContractConnection> connectionTask,
        string? channel = null,
        string? group = null,
        bool ignoreMessageHeader = false,
        MessageFilters<TQuery>? messageFilters = null,
        CancellationToken cancellationToken = default
    )
    where TConsumer : IQueryResponseAsyncConsumer<TQuery, TQueryResponse>
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterQueryResponseAsyncConsumerAsync<TQuery, TQueryResponse, TConsumer>(
            channel, group, ignoreMessageHeader, messageFilters, cancellationToken
        ).ConfigureAwait(false);

        return connection;
    }
    /// <summary>
    /// Called to register a QueryResponseAsyncConsumer into the contract connection. 
    /// </summary>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="consumerType">The type instance to be constructed and registered into the system.  It must implement IQueryResponseAsyncConsumer&lt;Q,R&gt;.</param>
    /// <param name="channel">Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class.</param>
    /// <param name="group">The subscription group if desired (typically used when multiple instances of the same system are running)</param>
    /// <param name="ignoreMessageHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterQueryResponseAsyncConsumerAsync(
        this ValueTask<IMultiServiceContractConnection> connectionTask,
        Type consumerType,
        string? channel = null,
        string? group = null,
        bool ignoreMessageHeader = false,
        CancellationToken cancellationToken = default
    )
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterQueryResponseAsyncConsumerAsync(
            consumerType, channel, group, ignoreMessageHeader, cancellationToken
        ).ConfigureAwait(false);

        return connection;
    }
    #endregion

    #region MessageContext
    /// <summary>
    /// Called to register a Message Context with the given connection
    /// </summary>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="messageContext">The Message Context (that the code generator has built upon) to register</param>
    /// <returns>The Contract Connection</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterMessageContextAsync(
        this ValueTask<IMultiServiceContractConnection> connectionTask,
        MQContractMessageContext messageContext
    )
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterMessageContextAsync(
            messageContext
        ).ConfigureAwait(false);

        return connection;
    }
    #endregion

    #region Middleware
    /// <summary>
    /// Register a middleware of a given type T to be used by the contract connection
    /// </summary>
    /// <typeparam name="TMiddleware">The type of middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware</typeparam>
    /// <param name="connectionTask">Original Connection task</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterMiddlewareAsync<TMiddleware>(
        this ValueTask<IMultiServiceContractConnection> connectionTask
    )
        where TMiddleware : IMiddleware
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterMiddlewareAsync<TMiddleware>().ConfigureAwait(false);
        return connection;
    }
    /// <summary>
    /// Register a middleware of a given type 
    /// </summary>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="middleware">The type of middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware or IBeforeEncodeSpecificTypeMiddleware&lt;&gt; or IAfterDecodeSpecificTypeMiddleware&lt;&gt;</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterMiddlewareAsync(
        this ValueTask<IMultiServiceContractConnection> connectionTask, 
        Type middleware
    )
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterMiddlewareAsync(middleware).ConfigureAwait(false);
        return connection;
    }
    /// <summary>
    /// Register a middleware instance
    /// </summary>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="instance">The middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware or IBeforeEncodeSpecificTypeMiddleware&lt;&gt; or IAfterDecodeSpecificTypeMiddleware&lt;&gt;</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterMiddlewareAsync(
        this ValueTask<IMultiServiceContractConnection> connectionTask, 
        IMiddleware instance
    )
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterMiddlewareAsync(instance).ConfigureAwait(false);
        return connection;
    }
    /// <summary>
    /// Register a middleware of a given type T to be used by the contract connection
    /// </summary>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="constructInstance">Callback to create the instance</param>
    /// <typeparam name="TMiddleware">The type of middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware</typeparam>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterMiddlewareAsync<TMiddleware>(
        this ValueTask<IMultiServiceContractConnection> connectionTask, 
        Func<TMiddleware> constructInstance
    )
        where TMiddleware : IMiddleware
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterMiddlewareAsync(constructInstance).ConfigureAwait(false);
        return connection;
    }
    /// <summary>
    /// Register a middleware through a construct instance function
    /// </summary>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="constructInstance">Callback to create the instance.  The object returned must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware or IBeforeEncodeSpecificTypeMiddleware&lt;&gt; or IAfterDecodeSpecificTypeMiddleware&lt;&gt;</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterMiddlewareAsync(
        this ValueTask<IMultiServiceContractConnection> connectionTask, 
        Func<IMiddleware> constructInstance
    )
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterMiddlewareAsync(constructInstance).ConfigureAwait(false);
        return connection;
    }
    /// <summary>
    /// Register a middleware of a given type T to be used by the contract connection
    /// </summary>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="constructInstance">Callback to create the instance.  The object returned it must implement IBeforeEncodeSpecificTypeMiddleware&lt;M&gt; or IAfterDecodeSpecificTypeMiddleware&lt;M&gt;</param>
    /// <typeparam name="TMessage">The message type that this middleware is specifically called for</typeparam>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterMiddlewareAsync<TMessage>(
        this ValueTask<IMultiServiceContractConnection> connectionTask,
        Func<ISpecificTypeMiddleware<TMessage>> constructInstance
    )
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterMiddlewareAsync<TMessage>(constructInstance).ConfigureAwait(false);
        return connection;
    }
    /// <summary>
    /// Register a middleware of a given type T to be used by the contract connection
    /// </summary>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="instance">The middle ware to register, it must implement it must implement IBeforeEncodeSpecificTypeMiddleware&lt;M&gt; or IAfterDecodeSpecificTypeMiddleware&lt;M&gt;</param>
    /// <typeparam name="TMessage">The message type that this middleware is specifically called for</typeparam>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterMiddlewareAsync<TMessage>(
        this ValueTask<IMultiServiceContractConnection> connectionTask, 
        ISpecificTypeMiddleware<TMessage> instance
    )
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterMiddlewareAsync<TMessage>(instance).ConfigureAwait(false);
        return connection;
    }
    /// <summary>
    /// Register a middleware of a given type T to be used by the contract connection
    /// </summary>
    /// <typeparam name="TMiddleware">The type of middle ware to register, it must implement IBeforeEncodeSpecificTypeMiddleware&lt;M&gt; or IAfterDecodeSpecificTypeMiddleware&lt;M&gt;</typeparam>
    /// <typeparam name="TMessage">The message type that this middleware is specifically called for</typeparam>
    /// <param name="connectionTask">Original Connection task</param>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterMiddlewareAsync<TMiddleware, TMessage>(
        this ValueTask<IMultiServiceContractConnection> connectionTask
    )
        where TMiddleware : ISpecificTypeMiddleware<TMessage>
    {
        var connection = await connectionTask.ConfigureAwait(false);    
        await connection.RegisterMiddlewareAsync<TMiddleware, TMessage>().ConfigureAwait(false);
        return connection;
    }
    /// <summary>
    /// Register a middleware of a given type T to be used by the contract connection
    /// </summary>
    /// <param name="connectionTask">Original Connection task</param>
    /// <param name="constructInstance">Callback to create the instance</param>
    /// <typeparam name="TMiddleware">The type of middle ware to register, it must implement IBeforeEncodeSpecificTypeMiddleware&lt;M&gt; or IAfterDecodeSpecificTypeMiddleware&lt;M&gt;</typeparam>
    /// <typeparam name="TMessage">The message type that this middleware is specifically called for</typeparam>
    /// <returns>The Contract Connection instance to allow chaining calls</returns>
    public static async ValueTask<IMultiServiceContractConnection> RegisterMiddlewareAsync<TMiddleware, TMessage>(
        this ValueTask<IMultiServiceContractConnection> connectionTask, 
        Func<TMiddleware> constructInstance
    )
        where TMiddleware : ISpecificTypeMiddleware<TMessage>
    {
        var connection = await connectionTask.ConfigureAwait(false);
        await connection.RegisterMiddlewareAsync<TMiddleware, TMessage>(constructInstance).ConfigureAwait(false);
        return connection;
    }
    #endregion
}
