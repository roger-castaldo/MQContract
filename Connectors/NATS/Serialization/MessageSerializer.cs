using NATS.Client.Core;
using System.Buffers;
using ProtoBuf;
using MQContract.NATS.Messages;

namespace MQContract.NATS.Serialization
{
    internal class MessageSerializer<T> : INatsSerializer<T>
    {
        public static readonly INatsSerializer<T> Default = new MessageSerializer<T>();

        public T? Deserialize(in ReadOnlySequence<byte> buffer)
            => Serializer.Deserialize<T>(buffer);

        public void Serialize(IBufferWriter<byte> bufferWriter, T value)
            => Serializer.Serialize<T>(bufferWriter, value);
    }
}
