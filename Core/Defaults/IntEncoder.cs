namespace MQContract.Defaults;

internal class IntEncoder : ABitEncoder<int>
{
    protected override int ByteSize => sizeof(int);

    protected override int ConvertValue(ReadOnlySpan<byte> value)
        => BitConverter.ToInt32(value);

    protected override byte[] ConvertValue(int value)
        => BitConverter.GetBytes(value);
}
