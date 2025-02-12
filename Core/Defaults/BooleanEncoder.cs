using MQContract.Interfaces.Encoding;

namespace MQContract.Defaults
{
    internal class BooleanEncoder : IMessageTypeEncoder<bool>
    {
        async ValueTask<bool> IMessageTypeEncoder<bool>.DecodeAsync(Stream stream)
            => BitConverter.ToBoolean(await BitConverterHelper.StreamToByteArray(stream));

        ValueTask<byte[]> IMessageTypeEncoder<bool>.EncodeAsync(bool message)
            => ValueTask.FromResult(BitConverter.GetBytes(message));
    }
}
