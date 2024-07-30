using MQContract.Interfaces.Messages;

namespace MQContract.Messages
{
    public record ServiceQueryResult(string ID, IMessageHeader Header,string MessageTypeID,ReadOnlyMemory<byte> Data)
        : IEncodedMessage
    {
    }
}
