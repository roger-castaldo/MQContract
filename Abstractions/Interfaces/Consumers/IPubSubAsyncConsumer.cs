namespace MQContract.Interfaces.Consumers
{
    public interface IPubSubAsyncConsumer<T> : IBaseConsumer
    {
        ValueTask MessageReceivedAsync(IReceivedMessage<T> message);
        
    }
}
