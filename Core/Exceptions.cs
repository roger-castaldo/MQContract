namespace MQContract
{
    /// <summary>
    /// Thrown when an incoming data message causes a null object return from a converter
    /// </summary>
    public class MessageConversionException : Exception
    {
        internal MessageConversionException(Type messageType)
            : base($"The attempt to convert the incoming message resulted in a null object.[MessageType:{messageType.FullName}]") { }
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

        internal SubscriptionFailedException(Exception innerException)
            : base("Failed to establish subscription through service connection", innerException)
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
    public class QueryTimeoutException : TimeoutException
    {
        internal QueryTimeoutException()
            : base("Query Response request timed out") { }
    }

    /// <summary>
    /// Thrown when a query call message is received without proper data
    /// </summary>
    public class InvalidQueryResponseMessageReceivedException : Exception
    {
        internal InvalidQueryResponseMessageReceivedException()
            : base("A service message was received on a query response channel without the proper data") { }
    }

    /// <summary>
    /// Thrown from the Mapped Connection when no connections match the search criteria making the requested action impossible to do
    /// </summary>
    public class NoConnectionMatchException : Exception
    {
        internal NoConnectionMatchException()
            : base("No service connection matched the required criteria so unable to process") { }
    }

    /// <summary>
    /// Thrown from the Mapped Connection when more than 1 connection matches the search criteria making the requested action impossible to do
    /// </summary>
    public class TooManyConnectionMatchesException : Exception
    {
        internal TooManyConnectionMatchesException()
            : base("More than 1 underlying service connection matched the required criteria so unable to process") { }
    }

    /// <summary>
    /// Thrown from the ContractedConnection or the MappedConnection when there is no underlying service that supports the Ping call
    /// </summary>
    public class PingNotSupportedException : NotSupportedException
    {
        internal PingNotSupportedException()
            : base("The underlying service does not support Ping")
        { }
    }

    /// <summary>
    /// Thrown from a ContractConnection when an attempt to Register a given consumer through Type is not valid because the type does not implement the appropriate interface
    /// </summary>
    public class InvalidConsumerTypeException : NotSupportedException
    {
        internal InvalidConsumerTypeException(Type consumerType, Type interfaceType)
            : base($"Unable to register consumer of Type {consumerType.FullName} because it does not implement the interface {interfaceType.Name}")
        { }
    }

    /// <summary>
    /// Thrown from a Resiliant Contract Connection when an attempt to create a policy is made but there is not any valid arguments
    /// </summary>
    public class InvalidPolicyArgumentsException : ArgumentException
    {
        internal InvalidPolicyArgumentsException(string[] argumentNames)
            : base($"You must supply at least a {string.Join(" or a ", argumentNames)}") { }
    }

    /// <summary>
    /// Thrown from a Resiliant Contract Connection when an attempt to create a policy is made but the retry count is higher than the circuit break count
    /// </summary>
    public class InvalidRetryCircuitBreakTriggersException : ArgumentException
    {
        internal InvalidRetryCircuitBreakTriggersException(string retryArgumentName, string circuitBreakArumentName)
            : base($"The value for {retryArgumentName} should be less than {circuitBreakArumentName}") { }
    }

    /// <summary>
    /// Thrown from a Contract Connection when an attempt is made to register a middleware that does not implement any middleware interfaces
    /// </summary>
    public class InvalidMiddlewareException : ArgumentException
    {
        internal InvalidMiddlewareException(Type middlewareType)
            : base($"The type {middlewareType} does not implement any of the available middleware interfaces") { }
    }

    /// <summary>
    /// Thrown when the registration of a consumer failes
    /// </summary>
    public sealed class ConsumerRegistrationFailedException
        : Exception
    {
        internal ConsumerRegistrationFailedException(string consumerName, Type consumerType, Exception exception)
            : base($"Failed to register a {consumerName} of type {consumerType}", exception) { }
    }

    /// <summary>
    /// Thrown when dynamic code is not supported but a call that requires it is made
    /// </summary>
    public sealed class DynamicCodeNotSupportedException
        : Exception
    {
        private DynamicCodeNotSupportedException(string message)
            : base(message) { }

        internal static void ThrowIfDynamicCodeIsBlocked(string message)
        {
            if (!DynamicCodeGate.IsSupported)
                throw new DynamicCodeNotSupportedException(message);
        }

    }
}
