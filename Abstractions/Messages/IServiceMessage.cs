namespace MQContract.Messages
{
    public interface IServiceMessage : IMessage
    {
        string MessageTypeID { get; }
        string Channel { get; }
        IMessageHeader Header { get; }
        ReadOnlyMemory<byte> Data { get; }
    }
}
