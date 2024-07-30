using MQContract.Messages;

namespace MQContract.Interfaces.Messages
{
    public interface IEncodedMessage
    {
        IMessageHeader Header { get; }
        string MessageTypeID { get; }
        ReadOnlyMemory<byte> Data { get; }
    }
}
