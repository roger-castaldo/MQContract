using MQContract.Interfaces.Encoding;

namespace MQContract.Defaults
{
    internal class HalfEncoder : IMessageTypeEncoder<Half>
    {
        async ValueTask<Half> IMessageTypeEncoder<Half>.DecodeAsync(Stream stream)
        => BitConverter.ToHalf(await BitConverterHelper.StreamToByteArray(stream));

        ValueTask<byte[]> IMessageTypeEncoder<Half>.EncodeAsync(Half message)
        => ValueTask.FromResult(BitConverter.GetBytes(message));
    }
}
