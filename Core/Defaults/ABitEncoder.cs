using MQContract.Interfaces.Encoding;

namespace MQContract.Defaults
{
    internal abstract class ABitEncoder<T>
        : IMessageTypeEncoder<T>, IMessageTypeEncoder<T[]>, IMessageTypeEncoder<IEnumerable<T>>
    {
        protected abstract int ByteSize { get; }
        protected abstract T ConvertValue(ReadOnlySpan<byte> value);
        protected abstract byte[] ConvertValue(T value);

        async ValueTask<T?> IMessageTypeEncoder<T>.DecodeAsync(Stream stream)
            => ConvertValue(await BitConverterHelper.StreamToByteArray(stream));

        async ValueTask<T[]?> IMessageTypeEncoder<T[]>.DecodeAsync(Stream stream)
            => (await ((IMessageTypeEncoder<IEnumerable<T>>)this).DecodeAsync(stream))?.ToArray();

        ValueTask<IEnumerable<T>?> IMessageTypeEncoder<IEnumerable<T>>.DecodeAsync(Stream stream)
        {
            using var reader = new BinaryReader(stream);
            var result = new List<T>();
            var buffer = new byte[ByteSize];
            while (ByteSize == reader.Read(buffer, 0, buffer.Length))
                result.Add(ConvertValue(buffer));
            return ValueTask.FromResult<IEnumerable<T>?>(result);
        }

        ValueTask<byte[]> IMessageTypeEncoder<T>.EncodeAsync(T message)
            => ValueTask.FromResult(ConvertValue(message));

        async ValueTask<byte[]> IMessageTypeEncoder<T[]>.EncodeAsync(T[] message)
            => await ((IMessageTypeEncoder<IEnumerable<T>>)this).EncodeAsync(message);

        ValueTask<byte[]> IMessageTypeEncoder<IEnumerable<T>>.EncodeAsync(IEnumerable<T> message)
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
