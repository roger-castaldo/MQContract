using MQContract.Messages;

namespace MQContract.Interfaces.Consumers
{
    public interface IQueryResponseAsyncConsumer<Q,R> : IBaseConsumer
    {
        ValueTask<QueryResponseMessage<R>> MessageReceivedAsync(IReceivedMessage<Q> message);
    }
}
