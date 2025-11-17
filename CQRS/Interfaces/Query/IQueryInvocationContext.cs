namespace MQContract.CQRS.Interfaces.Query
{
    /// <summary>
    /// Represents a given execution context for a query
    /// </summary>
    /// <typeparam name="TQuery">The type of query housed within this context</typeparam>
    public interface IQueryInvocationContext<TQuery> : IInvocationContext
        where TQuery : IQuery
    {
        /// <summary>
        /// The query for this invocation context instance
        /// </summary>
        TQuery Query { get; }
    }
}
