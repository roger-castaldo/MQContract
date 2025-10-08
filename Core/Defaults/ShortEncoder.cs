namespace MQContract.Defaults
{
    internal class ShortEncoder : ABitEncoder<short>
    {
        protected override int ByteSize => sizeof(short);

        protected override short ConvertValue(ReadOnlySpan<byte> value)
            => BitConverter.ToInt16(value);

        protected override byte[] ConvertValue(short value)
            => BitConverter.GetBytes(value);
    }
}
