namespace MQContract.CQRS.Interfaces.Query
{
    internal interface IFilteredQueryProcessor<TQuery, TQueryResponse> : IQueryProcessor<TQuery, TQueryResponse>
        where TQuery : IQuery
        where TQueryResponse : IQueryResponse
    {
        Func<TQuery, Context, ValueTask<MessageFilterResult>> Filter { get; }
    }
}
