namespace MQContract.Defaults;

internal class DoubleEncoder : ABitEncoder<double>
{
    protected override int ByteSize => sizeof(double);

    protected override double ConvertValue(ReadOnlySpan<byte> value)
        => BitConverter.ToDouble(value);

    protected override byte[] ConvertValue(double value)
        => BitConverter.GetBytes(value);
}
