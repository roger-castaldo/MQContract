using MQContract.Interfaces.Encoding;

namespace MQContract.Defaults
{
    internal class IntEncoder : IMessageTypeEncoder<int>
    {
        async ValueTask<int> IMessageTypeEncoder<int>.DecodeAsync(Stream stream)
        => BitConverter.ToInt32(await BitConverterHelper.StreamToByteArray(stream));

        ValueTask<byte[]> IMessageTypeEncoder<int>.EncodeAsync(int message)
        => ValueTask.FromResult(BitConverter.GetBytes(message));
    }
}
