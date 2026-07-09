namespace MQContract.Defaults;

internal class BooleanEncoder : ABitEncoder<bool>
{
    protected override int ByteSize => 1;

    protected override bool ConvertValue(ReadOnlySpan<byte> value)
        => BitConverter.ToBoolean(value);

    protected override byte[] ConvertValue(bool value)
        => BitConverter.GetBytes(value);
}
