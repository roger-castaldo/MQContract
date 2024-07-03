using MQContract.Messages;

namespace MQContract.Interfaces
{
    public interface IMessage<out T>
        where T : class
    {
        string ID { get; }
        T Message { get; }
        IMessageHeader Headers { get; }
    }
}
