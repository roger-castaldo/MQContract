using MQContract.Messages;

namespace MQContract.CQRS
{
    /// <summary>
    /// Thrown when a command execution call throws an error from the underlying contract connection
    /// </summary>
    public class CommandCallException : Exception
    {
        /// <summary>
        /// The error that occured while attempting to execute a given command
        /// </summary>
        public ErrorMessage Error { get; private init; }

        internal CommandCallException(ErrorMessage errorMessage)
            : base()
        {
            Error = errorMessage;
        }
    }

    /// <summary>
    /// Thrown when a command call's timeout is exceeded prior to a response being returned
    /// </summary>
    public class CommandTimeoutException : Exception
    {
        internal CommandTimeoutException(Exception innerException)
            : base("Command request timed out", innerException) { }
    }

    /// <summary>
    /// Thrown when a query execution call throws an error from the underlying contract connection
    /// </summary>
    public class QueryCallException : Exception
    {
        /// <summary>
        /// The error that occured while attempting to execute a given query
        /// </summary>
        public ErrorMessage Error { get; private init; }

        internal QueryCallException(ErrorMessage errorMessage)
            : base()
        {
            Error = errorMessage;
        }
    }

    /// <summary>
    /// Thrown when an invalid contract connection type is supplied in an attempt to create a CQRS connection
    /// </summary>
    public class InvalidConnectionException : InvalidCastException
    {
        internal InvalidConnectionException(Type type)
            : base($"The type of {type.FullName} is not a valid connection type to use with CQRS") { }
    }
}
