namespace MQContract.ServiceAbstractions.Messages
{
    public interface IServiceMessage
    {
        string ID { get; }
        string MessageTypeID { get; }
        string Channel { get; }
        IMessageHeader Header { get; }
        ReadOnlyMemory<byte> Data { get; }
    }
}
