using MQContract.Interfaces.Messages;
using MQContract.Messages;

namespace CodeGenTesting.Messages
{
    internal class DummyEncodedMessage(string id, byte[] data) : IEncodedMessage
    {
        MessageHeader IEncodedMessage.Header => new([]);

        string IEncodedMessage.MessageTypeID => id;

        ReadOnlyMemory<byte> IEncodedMessage.Data => data;
    }
}
