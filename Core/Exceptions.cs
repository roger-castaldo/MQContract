namespace MQContract
{
    /// <summary>
    /// Thrown when an incoming data message causes a null object return from a converter
    /// </summary>
    public class MessageConversionException : Exception
    {
        internal MessageConversionException(Type messageType, Type converterType)
            : base($"The attempt to convert the incoming message resulted in a null object.[MessageType:{messageType.FullName},ConverterType:{converterType.FullName}]") { }
    }

    /// <summary>
    /// Thrown when a QueryResponse type message is attempted without specifying the response type and there is no Response Type attribute for the query class.
    /// </summary>
    public class UnknownResponseTypeException : ArgumentNullException
    {
        internal UnknownResponseTypeException(string paramName, Type messageType)
            : base(paramName, $"The attempt to call a query response with the incoming message of type {messageType.FullName} does not have a determined response type.") { }
    }
}
