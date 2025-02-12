using MQContract.Messages;

namespace MQContract.Interfaces.Consumers
{
    public interface IQueryResponseConsumer<Q,R> : IBaseConsumer
    {
        QueryResponseMessage<R> MessageReceived(IReceivedMessage<Q> message);
    }
}
