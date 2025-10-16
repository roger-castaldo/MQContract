namespace MQContract.Defaults
{
    internal class LongEncoder : ABitEncoder<long>
    {
        protected override int ByteSize => sizeof(long);

        protected override long ConvertValue(ReadOnlySpan<byte> value)
            => BitConverter.ToInt64(value);

        protected override byte[] ConvertValue(long value)
            => BitConverter.GetBytes(value);

    }
}
