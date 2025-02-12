using MQContract.Interfaces.Encoding;

namespace MQContract.Defaults
{
    internal class UShortEncoder : IMessageTypeEncoder<ushort>
    {
        async ValueTask<ushort> IMessageTypeEncoder<ushort>.DecodeAsync(Stream stream)
        => BitConverter.ToUInt16(await BitConverterHelper.StreamToByteArray(stream));

        ValueTask<byte[]> IMessageTypeEncoder<ushort>.EncodeAsync(ushort message)
        => ValueTask.FromResult(BitConverter.GetBytes(message));
    }
}
