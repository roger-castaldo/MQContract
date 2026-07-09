using MQContract.CQRS.Interfaces.Query;
using MQContract.Interfaces;

namespace MQContract.CQRS.Contexts;

internal sealed class QueryInvocationContext<TQuery>(IReceivedMessage<TQuery> receivedMessage, CqrsConnection connection)
    : AInvocationContext<TQuery>(receivedMessage, connection), IQueryInvocationContext<TQuery>
    where TQuery : IQuery
{
    TQuery IQueryInvocationContext<TQuery>.Query => Message;
}
