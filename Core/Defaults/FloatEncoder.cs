namespace MQContract.Defaults;

internal class FloatEncoder : ABitEncoder<float>
{
    protected override int ByteSize => sizeof(float);

    protected override float ConvertValue(ReadOnlySpan<byte> value)
        => BitConverter.ToSingle(value);

    protected override byte[] ConvertValue(float value)
        => BitConverter.GetBytes(value);
}
