namespace MQContract.CQRS.Interfaces.Query
{
    public interface IQueryProcessor<TQuery,TQueryResponse> : IProcessor
        where TQuery : IQuery
        where TQueryResponse : IQueryResponse
    {
        ValueTask<TQueryResponse> ProcessQueryAsync(IQueryInvocationContext<TQuery> invocationContext, CancellationToken cancellationToken);
    }
}
