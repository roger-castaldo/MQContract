namespace MQContract
{
    public sealed class InvalidChannelOptionsTypeException
        : InvalidCastException
    {
        public InvalidChannelOptionsTypeException(IEnumerable<Type> expectedTypes,Type recievedType) : 
            base($"Expected Channel Options of Types[{string.Join(',',expectedTypes.Select(t=>t.FullName))}] but recieved {recievedType.FullName}")
        {
        }

        public InvalidChannelOptionsTypeException(Type expectedType, Type recievedType) :
            base($"Expected Channel Options of Type {expectedType.FullName} but recieved {recievedType.FullName}")
        {
        }
    }
}
