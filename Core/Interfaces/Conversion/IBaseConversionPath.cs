namespace MQContract.Interfaces.Conversion
{
    internal interface IBaseConversionPath
    {
        bool CanConvert(Type sourceType);
    }
}
