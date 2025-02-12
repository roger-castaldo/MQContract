using MQContract.Interfaces.Encoding;

namespace MQContract.Defaults
{
    internal class CharEncoder : IMessageTypeEncoder<char>
    {
        async ValueTask<char> IMessageTypeEncoder<char>.DecodeAsync(Stream stream)
            => BitConverter.ToChar(await BitConverterHelper.StreamToByteArray(stream));

        ValueTask<byte[]> IMessageTypeEncoder<char>.EncodeAsync(char message)
            => ValueTask.FromResult(BitConverter.GetBytes(message));
    }
}
