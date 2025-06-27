using MQContract.Interfaces.Encoding;

namespace MQContract.Defaults
{
    internal class LongEncoder : IMessageTypeEncoder<long>
    {
        async ValueTask<long> IMessageTypeEncoder<long>.DecodeAsync(Stream stream)
        => BitConverter.ToInt64(await BitConverterHelper.StreamToByteArray(stream));

        ValueTask<byte[]> IMessageTypeEncoder<long>.EncodeAsync(long message)
        => ValueTask.FromResult(BitConverter.GetBytes(message));
    }
}
