namespace MQContract.CQRS.Interfaces.Query
{
    /// <summary>
    /// Defines a query processor for the given type of query that expects the given response
    /// </summary>
    /// <example>
    /// <code>
    /// public class GetUserProcessor : IQueryProcessor&lt;GetUserQuery, User&gt;
    /// {
    ///     public async ValueTask&lt;User&gt; ProcessQueryAsync(IQueryInvocationContext&lt;GetUserQuery&gt; context, CancellationToken cancellationToken)
    ///     {
    ///         var query = context.Query;
    ///         // Process the query (e.g., retrieve from database)
    ///         return new User(query.UserId, "John", "john@example.com");
    ///     }
    /// 
    ///     void IProcessor.ErrorRecieved(Exception error)
    ///     {
    ///         Console.WriteLine($"Error: {error.Message}");
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="TQuery">The type of query</typeparam>
    /// <typeparam name="TQueryResponse">The type of response from the query</typeparam>
    public interface IQueryProcessor<TQuery,TQueryResponse> : IProcessor
        where TQuery : IQuery
    {
        /// <summary>
        /// the callback executed against the query
        /// </summary>
        /// <param name="invocationContext">The current invocation context for this query instance</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The result from the query execution</returns>
        ValueTask<TQueryResponse> ProcessQueryAsync(IQueryInvocationContext<TQuery> invocationContext, CancellationToken cancellationToken);
    }
}
