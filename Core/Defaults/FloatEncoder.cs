using MQContract.Interfaces.Encoding;

namespace MQContract.Defaults
{
    internal class FloatEncoder : IMessageTypeEncoder<float>
    {
        async ValueTask<float> IMessageTypeEncoder<float>.DecodeAsync(Stream stream)
        => BitConverter.ToSingle(await BitConverterHelper.StreamToByteArray(stream));

        ValueTask<byte[]> IMessageTypeEncoder<float>.EncodeAsync(float message)
        => ValueTask.FromResult(BitConverter.GetBytes(message));
    }
}
