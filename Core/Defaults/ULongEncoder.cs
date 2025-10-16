namespace MQContract.Defaults
{
    internal class ULongEncoder : ABitEncoder<ulong>
    {
        protected override int ByteSize => sizeof(ulong);

        protected override ulong ConvertValue(ReadOnlySpan<byte> value)
            => BitConverter.ToUInt64(value);

        protected override byte[] ConvertValue(ulong value)
            => BitConverter.GetBytes(value);
    }
}
