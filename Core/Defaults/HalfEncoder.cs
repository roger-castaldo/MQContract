using MQContract.Interfaces.Encoding;

namespace MQContract.Defaults
{
    internal class HalfEncoder : ABitEncoder<Half>
    {
        protected override int ByteSize => 2;

        protected override Half ConvertValue(ReadOnlySpan<byte> value)
            => BitConverter.ToHalf(value);

        protected override byte[] ConvertValue(Half value)
            => BitConverter.GetBytes(value);
    }
}
