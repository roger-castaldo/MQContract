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

    /// <summary>
    /// Thrown when a Subscription has failed to be established/created
    /// </summary>
    public class SubscriptionFailedException : Exception
    {
        internal SubscriptionFailedException()
            : base("Failed to establish subscription through service connection")
        { }
    }

    /// <summary>
    /// Thrown when a call is made but the system is unable to detect the channel
    /// </summary>
    public class MessageChannelNullException : ArgumentNullException
    {
        internal MessageChannelNullException()
            : base("channel", "message must have a channel value") { }
    }

    /// <summary>
    /// Thrown when a Query call is made and there is an error in the response
    /// </summary>
    public class QueryResponseException : Exception
    {
        internal QueryResponseException(string message)
            : base(message) { }
    }

    /// <summary>
    /// Thrown when a query call is being made to a service that does not support query response and the listener cannot be created
    /// </summary>
    public class QueryExecutionFailedException : Exception
    {
        internal QueryExecutionFailedException()
            : base("Failed to execute query") { }
    }

    /// <summary>
    /// Thrown when a query call times out waiting for the response
    /// </summary>
    public class QueryTimeoutException : Exception
    {
        internal QueryTimeoutException()
            : base("Query Response request timed out") { }
    }

    /// <summary>
    /// Thrown when a query call message is received without proper data
    /// </summary>
    public class InvalidQueryResponseMessageReceived : Exception
    {
        internal InvalidQueryResponseMessageReceived()
            : base("A service message was received on a query response channel without the proper data") { }
    }
}
