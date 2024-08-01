namespace MQContract
{
    /// <summary>
    /// An exception thrown when the options supplied to an underlying system connection are not of an expected type.
    /// </summary>
    public sealed class InvalidChannelOptionsTypeException
        : InvalidCastException
    {
        /// <summary>
        /// Constructor for the possibility of multiple types that can be used and a not valid type is passed
        /// </summary>
        /// <param name="expectedTypes">The possible types that the channel options object can be</param>
        /// <param name="recievedType">The type that was recieved</param>
        public InvalidChannelOptionsTypeException(IEnumerable<Type> expectedTypes,Type recievedType) : 
            base($"Expected Channel Options of Types[{string.Join(',',expectedTypes.Select(t=>t.FullName))}] but recieved {recievedType.FullName}")
        {
        }

        /// <summary>
        /// Constructor for the possibility of a single type that can be used and a not valid type is passed
        /// </summary>
        /// <param name="expectedType">The possible type that the channel options object can be</param>
        /// <param name="recievedType">The type that was recieved</param>
        public InvalidChannelOptionsTypeException(Type expectedType, Type recievedType) :
            base($"Expected Channel Options of Type {expectedType.FullName} but recieved {recievedType.FullName}")
        {
        }
    }
}
