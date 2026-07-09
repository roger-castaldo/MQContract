using MQContract.CQRS.Contexts;
using MQContract.CQRS.Interfaces.Query;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;
using MQContract.Messages;

namespace MQContract.CQRS.Consumers;

internal class QueryResponseConsumer<TQuery, TQueryResponse>(IQueryProcessor<TQuery, TQueryResponse> queryProcessor, CqrsConnection connection)
    : IQueryResponseAsyncConsumer<TQuery, TQueryResponse>
    where TQuery : IQuery
{
    void IBaseConsumer.ErrorRecieved(Exception error)
        => queryProcessor.ErrorRecieved(error);

    async ValueTask<QueryResponseMessage<TQueryResponse>> IQueryResponseAsyncConsumer<TQuery, TQueryResponse>.MessageReceivedAsync(IReceivedMessage<TQuery> message)
    {
        await using var context = new QueryInvocationContext<TQuery>(message, connection);
        var result = await queryProcessor.ProcessQueryAsync(context, context.CancellationTokenSource.Token);
        return new(result, context.Headers);
    }
}
