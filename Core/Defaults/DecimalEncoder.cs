namespace MQContract.Defaults;

internal class DecimalEncoder : ABitEncoder<decimal>
{
    private const int BitsPerDecimal = 4;

    protected override int ByteSize => BitsPerDecimal*sizeof(int);

    protected override decimal ConvertValue(ReadOnlySpan<byte> value)
    {
        var bits = new int[BitsPerDecimal];
        for (var i = 0; i<bits.Length; i++)
            bits[i] = BitConverter.ToInt32(value.Slice(i*sizeof(int), 4));

        return new decimal(bits);
    }

    protected override byte[] ConvertValue(decimal value)
    {
        var result = new byte[sizeof(int)*BitsPerDecimal];

        var bits = decimal.GetBits(value);

        for (var i = 0; i<bits.Length; i++)
            Buffer.BlockCopy(BitConverter.GetBytes(bits[i]), 0, result, i*sizeof(int), sizeof(int));

        return result;
    }

}
