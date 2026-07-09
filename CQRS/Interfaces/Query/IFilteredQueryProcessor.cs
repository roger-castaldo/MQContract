namespace MQContract.CQRS.Interfaces.Query;

/// <summary>
/// Used to define a query processor that will filter incoming messages based on the query
/// </summary>
/// <typeparam name="TQuery">The type of query that this processor handles</typeparam>
/// <typeparam name="TQueryResponse">The type of response that this query processor provides</typeparam>
public interface IFilteredQueryProcessor<TQuery, TQueryResponse> : IQueryProcessor<TQuery, TQueryResponse>
    where TQuery : IQuery
{
    /// <summary>
    /// The filter callback expected to return a filter result and will be supplied the current 
    /// context and query instance
    /// </summary>
    Func<TQuery, Context, ValueTask<MessageFilterResult>> Filter { get; }
}
