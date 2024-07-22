namespace MQContract.Messages
{
    public interface IErrorServiceMessage : IServiceMessage
    {
        Exception Error { get; }
    }
}
