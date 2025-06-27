using MQContract.Interfaces.Middleware;

namespace MQContract.Interfaces
{
    /// <summary>
    /// Houses the middleware pieces for a given contract connection
    /// </summary>
    /// <typeparam name="CC">The underlying type that is being represented here which must be IBaseContractConnection, CC is used for method chaining.</typeparam>
    public interface IMiddlewareContractConnection<CC> : IResilientContractConnection<CC>
        where CC : IBaseContractConnection
    {
        /// <summary>
        /// Register a middleware of a given type T to be used by the contract connection
        /// </summary>
        /// <typeparam name="T">The type of middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware</typeparam>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        CC RegisterMiddleware<T>()
            where T : IMiddleware;
        /// <summary>
        /// Register a middleware of a given type 
        /// </summary>
        /// <param name="middleware">The type of middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware or IBeforeEncodeSpecificTypeMiddleware&lt;&gt; or IAfterDecodeSpecificTypeMiddleware&lt;&gt;</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        CC RegisterMiddleware(Type middleware);
        /// <summary>
        /// Register a middleware instance
        /// </summary>
        /// <param name="instance">The middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware or IBeforeEncodeSpecificTypeMiddleware&lt;&gt; or IAfterDecodeSpecificTypeMiddleware&lt;&gt;</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        CC RegisterMiddleware(IMiddleware instance);
        /// <summary>
        /// Register a middleware of a given type T to be used by the contract connection
        /// </summary>
        /// <param name="constructInstance">Callback to create the instance</param>
        /// <typeparam name="T">The type of middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware</typeparam>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        CC RegisterMiddleware<T>(Func<T> constructInstance)
            where T : IMiddleware;
        /// <summary>
        /// Register a middleware through a construct instance function
        /// </summary>
        /// <param name="constructInstance">Callback to create the instance.  The object returned must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware or IBeforeEncodeSpecificTypeMiddleware&lt;&gt; or IAfterDecodeSpecificTypeMiddleware&lt;&gt;</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        CC RegisterMiddleware(Func<IMiddleware> constructInstance);
        /// <summary>
        /// Register a middleware of a given type T to be used by the contract connection
        /// </summary>
        /// <param name="constructInstance">Callback to create the instance.  The object returned it must implement IBeforeEncodeSpecificTypeMiddleware&lt;M&gt; or IAfterDecodeSpecificTypeMiddleware&lt;M&gt;</param>
        /// <typeparam name="M">The message type that this middleware is specifically called for</typeparam>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        CC RegisterMiddleware<M>(Func<ISpecificTypeMiddleware<M>> constructInstance);
        /// <summary>
        /// Register a middleware of a given type T to be used by the contract connection
        /// </summary>
        /// <param name="instance">The middle ware to register, it must implement it must implement IBeforeEncodeSpecificTypeMiddleware&lt;M&gt; or IAfterDecodeSpecificTypeMiddleware&lt;M&gt;</param>
        /// <typeparam name="M">The message type that this middleware is specifically called for</typeparam>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        CC RegisterMiddleware<M>(ISpecificTypeMiddleware<M> instance);
        /// <summary>
        /// Register a middleware of a given type T to be used by the contract connection
        /// </summary>
        /// <typeparam name="T">The type of middle ware to register, it must implement IBeforeEncodeSpecificTypeMiddleware&lt;M&gt; or IAfterDecodeSpecificTypeMiddleware&lt;M&gt;</typeparam>
        /// <typeparam name="M">The message type that this middleware is specifically called for</typeparam>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        CC RegisterMiddleware<T, M>()
            where T : ISpecificTypeMiddleware<M>;
        /// <summary>
        /// Register a middleware of a given type T to be used by the contract connection
        /// </summary>
        /// <param name="constructInstance">Callback to create the instance</param>
        /// <typeparam name="T">The type of middle ware to register, it must implement IBeforeEncodeSpecificTypeMiddleware&lt;M&gt; or IAfterDecodeSpecificTypeMiddleware&lt;M&gt;</typeparam>
        /// <typeparam name="M">The message type that this middleware is specifically called for</typeparam>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        CC RegisterMiddleware<T, M>(Func<T> constructInstance)
            where T : ISpecificTypeMiddleware<M>;
    }
}
