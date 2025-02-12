namespace MQContract.Interfaces.Consumers
{
    public interface IBaseConsumer
    {
        void ErrorRecieved(Exception error);
    }
}
