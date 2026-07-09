namespace MQContract.Defaults;

internal class UIntEncoder : ABitEncoder<uint>
{
    protected override int ByteSize => sizeof(uint);

    protected override uint ConvertValue(ReadOnlySpan<byte> value)
        => BitConverter.ToUInt32(value);

    protected override byte[] ConvertValue(uint value)
        => BitConverter.GetBytes(value);
}
