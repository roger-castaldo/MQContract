namespace MQContract.Defaults
{
    internal class UShortEncoder : ABitEncoder<ushort>
    {
        protected override int ByteSize => sizeof(ushort);

        protected override ushort ConvertValue(ReadOnlySpan<byte> value)
            => BitConverter.ToUInt16(value);

        protected override byte[] ConvertValue(ushort value)
            => BitConverter.GetBytes(value);
    }
}
