using MQContract.Interfaces.Encoding;

namespace MQContract.Defaults
{
    internal class UIntEncoder : IMessageTypeEncoder<uint>
    {
        async ValueTask<uint> IMessageTypeEncoder<uint>.DecodeAsync(Stream stream)
        => BitConverter.ToUInt32(await BitConverterHelper.StreamToByteArray(stream));

        ValueTask<byte[]> IMessageTypeEncoder<uint>.EncodeAsync(uint message)
        => ValueTask.FromResult(BitConverter.GetBytes(message));
    }
}
