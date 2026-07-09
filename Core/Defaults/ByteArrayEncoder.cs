using MQContract.Interfaces.Encoding;

namespace MQContract.Defaults;

internal class ByteArrayEncoder : IMessageTypeEncoder<byte[]>
{
    async ValueTask<byte[]?> IMessageTypeEncoder<byte[]>.DecodeAsync(Stream stream)
        => await BitConverterHelper.StreamToByteArray(stream);

    ValueTask<byte[]> IMessageTypeEncoder<byte[]>.EncodeAsync(byte[] message)
        => ValueTask.FromResult(message);
}
