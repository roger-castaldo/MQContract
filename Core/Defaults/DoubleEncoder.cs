using MQContract.Interfaces.Encoding;

namespace MQContract.Defaults
{
    internal class DoubleEncoder : IMessageTypeEncoder<double>
    {
        async ValueTask<double> IMessageTypeEncoder<double>.DecodeAsync(Stream stream)
            => BitConverter.ToDouble(await BitConverterHelper.StreamToByteArray(stream));

        ValueTask<byte[]> IMessageTypeEncoder<double>.EncodeAsync(double message)
            => ValueTask.FromResult(BitConverter.GetBytes(message));
    }
}
