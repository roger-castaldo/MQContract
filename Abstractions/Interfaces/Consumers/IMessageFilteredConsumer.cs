using MQContract.Messages;

namespace MQContract.Interfaces.Consumers
{
    public interface IMessageFilteredConsumer<TMessage> : IBaseConsumer
    {
        Func<TMessage, MessageHeader, ValueTask<MessageFilterResult>> Filter { get; }
    }
}
