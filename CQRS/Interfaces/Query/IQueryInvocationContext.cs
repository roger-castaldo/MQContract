namespace MQContract.CQRS.Interfaces.Query
{
    public interface IQueryInvocationContext<Q> : IInvocationContext
        where Q : IQuery
    {
        Q Query { get; }
    }
}
