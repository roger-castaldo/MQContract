using MQContract.Interfaces.Middleware;

namespace MQContract.Interfaces
{
    /// <summary>
    /// Houses the middleware pieces for a given contract connection
    /// </summary>
    /// <typeparam name="TContractConnection">The underlying type that is being represented here which must be IBaseContractConnection, CC is used for method chaining.</typeparam>
    public interface IMiddlewareContractConnection<TContractConnection> : IResilientContractConnection<TContractConnection>
        where TContractConnection : IBaseContractConnection
    {
        /// <summary>
        /// Register a middleware of a given type T to be used by the contract connection
        /// </summary>
        /// <typeparam name="TMiddleware">The type of middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware</typeparam>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        ValueTask<TContractConnection> RegisterMiddlewareAsync<TMiddleware>()
            where TMiddleware : IMiddleware;
        /// <summary>
        /// Register a middleware of a given type 
        /// </summary>
        /// <param name="middleware">The type of middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware or IBeforeEncodeSpecificTypeMiddleware&lt;&gt; or IAfterDecodeSpecificTypeMiddleware&lt;&gt;</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        ValueTask<TContractConnection> RegisterMiddlewareAsync(Type middleware);
        /// <summary>
        /// Register a middleware instance
        /// </summary>
        /// <param name="instance">The middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware or IBeforeEncodeSpecificTypeMiddleware&lt;&gt; or IAfterDecodeSpecificTypeMiddleware&lt;&gt;</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        ValueTask<TContractConnection> RegisterMiddlewareAsync(IMiddleware instance);
        /// <summary>
        /// Register a middleware of a given type T to be used by the contract connection
        /// </summary>
        /// <param name="constructInstance">Callback to create the instance</param>
        /// <typeparam name="TMiddleware">The type of middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware</typeparam>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        ValueTask<TContractConnection> RegisterMiddlewareAsync<TMiddleware>(Func<TMiddleware> constructInstance)
            where TMiddleware : IMiddleware;
        /// <summary>
        /// Register a middleware through a construct instance function
        /// </summary>
        /// <param name="constructInstance">Callback to create the instance.  The object returned must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware or IBeforeEncodeSpecificTypeMiddleware&lt;&gt; or IAfterDecodeSpecificTypeMiddleware&lt;&gt;</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        ValueTask<TContractConnection> RegisterMiddlewareAsync(Func<IMiddleware> constructInstance);
        /// <summary>
        /// Register a middleware of a given type T to be used by the contract connection
        /// </summary>
        /// <param name="constructInstance">Callback to create the instance.  The object returned it must implement IBeforeEncodeSpecificTypeMiddleware&lt;M&gt; or IAfterDecodeSpecificTypeMiddleware&lt;M&gt;</param>
        /// <typeparam name="TMessage">The message type that this middleware is specifically called for</typeparam>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        ValueTask<TContractConnection> RegisterMiddlewareAsync<TMessage>(Func<ISpecificTypeMiddleware<TMessage>> constructInstance);
        /// <summary>
        /// Register a middleware of a given type T to be used by the contract connection
        /// </summary>
        /// <param name="instance">The middle ware to register, it must implement it must implement IBeforeEncodeSpecificTypeMiddleware&lt;M&gt; or IAfterDecodeSpecificTypeMiddleware&lt;M&gt;</param>
        /// <typeparam name="TMessage">The message type that this middleware is specifically called for</typeparam>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        ValueTask<TContractConnection> RegisterMiddlewareAsync<TMessage>(ISpecificTypeMiddleware<TMessage> instance);
        /// <summary>
        /// Register a middleware of a given type T to be used by the contract connection
        /// </summary>
        /// <typeparam name="TMiddleware">The type of middle ware to register, it must implement IBeforeEncodeSpecificTypeMiddleware&lt;M&gt; or IAfterDecodeSpecificTypeMiddleware&lt;M&gt;</typeparam>
        /// <typeparam name="TMessage">The message type that this middleware is specifically called for</typeparam>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        ValueTask<TContractConnection> RegisterMiddlewareAsync<TMiddleware, TMessage>()
            where TMiddleware : ISpecificTypeMiddleware<TMessage>;
        /// <summary>
        /// Register a middleware of a given type T to be used by the contract connection
        /// </summary>
        /// <param name="constructInstance">Callback to create the instance</param>
        /// <typeparam name="TMiddleware">The type of middle ware to register, it must implement IBeforeEncodeSpecificTypeMiddleware&lt;M&gt; or IAfterDecodeSpecificTypeMiddleware&lt;M&gt;</typeparam>
        /// <typeparam name="TMessage">The message type that this middleware is specifically called for</typeparam>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        ValueTask<TContractConnection> RegisterMiddlewareAsync<TMiddleware, TMessage>(Func<TMiddleware> constructInstance)
            where TMiddleware : ISpecificTypeMiddleware<TMessage>;
    }
}
