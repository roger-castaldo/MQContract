using MQContract.Interfaces.Messages;

namespace MQContract.Messages
{
    public record ServiceMessage(string ID,string MessageTypeID,string Channel,IMessageHeader Header,ReadOnlyMemory<byte> Data)
        : IEncodedMessage
    { }
}
