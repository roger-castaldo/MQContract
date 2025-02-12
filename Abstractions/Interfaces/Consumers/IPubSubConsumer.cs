namespace MQContract.Interfaces.Consumers
{
    public interface IPubSubConsumer<T> : IBaseConsumer
    {
        void MessageReceived(IReceivedMessage<T> message);
    }
}
