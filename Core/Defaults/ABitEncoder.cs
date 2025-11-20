using MQContract.Interfaces.Encoding;

namespace MQContract.Defaults
{
    internal abstract class ABitEncoder<TMessage>
        : IMessageTypeEncoder<TMessage>, IMessageTypeEncoder<TMessage[]>, IMessageTypeEncoder<IEnumerable<TMessage>>
    {
        protected abstract int ByteSize { get; }
        protected abstract TMessage ConvertValue(ReadOnlySpan<byte> value);
        protected abstract byte[] ConvertValue(TMessage value);

        async ValueTask<TMessage?> IMessageTypeEncoder<TMessage>.DecodeAsync(Stream stream)
            => ConvertValue(await BitConverterHelper.StreamToByteArray(stream));

        async ValueTask<TMessage[]?> IMessageTypeEncoder<TMessage[]>.DecodeAsync(Stream stream)
            => (await ((IMessageTypeEncoder<IEnumerable<TMessage>>)this).DecodeAsync(stream))?.ToArray();

        ValueTask<IEnumerable<TMessage>?> IMessageTypeEncoder<IEnumerable<TMessage>>.DecodeAsync(Stream stream)
        {
            using var reader = new BinaryReader(stream);
            var result = new List<TMessage>();
            var buffer = new byte[ByteSize];
            while (ByteSize == reader.Read(buffer, 0, buffer.Length))
                result.Add(ConvertValue(buffer));
            return ValueTask.FromResult<IEnumerable<TMessage>?>(result);
        }

        ValueTask<byte[]> IMessageTypeEncoder<TMessage>.EncodeAsync(TMessage message)
            => ValueTask.FromResult(ConvertValue(message));

        async ValueTask<byte[]> IMessageTypeEncoder<TMessage[]>.EncodeAsync(TMessage[] message)
            => await ((IMessageTypeEncoder<IEnumerable<TMessage>>)this).EncodeAsync(message);

        ValueTask<byte[]> IMessageTypeEncoder<IEnumerable<TMessage>>.EncodeAsync(IEnumerable<TMessage> message)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            foreach(var item in message)
                writer.Write(ConvertValue(item));
            writer.Flush();
            return ValueTask.FromResult(stream.ToArray());
        }
    }
}
