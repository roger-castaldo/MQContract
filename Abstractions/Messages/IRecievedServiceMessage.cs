namespace MQContract.Messages
{
    public interface IRecievedServiceMessage :IServiceMessage
    {
        DateTime RecievedTimestamp { get; }
    }
}
