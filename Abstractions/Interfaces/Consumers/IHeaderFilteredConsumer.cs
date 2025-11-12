using MQContract.Messages;

namespace MQContract.Interfaces.Consumers
{
    public interface IHeaderFilteredConsumer : IBaseConsumer
    {
        Func<MessageHeader, ValueTask<MessageFilterResult>> Filter { get; }
    }
}
