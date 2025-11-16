using MQContract.Messages;

namespace MQContract.CQRS
{
    public class CommandCallException : Exception
    {
        public ErrorMessage Error { get; private init; }
        
        internal CommandCallException(ErrorMessage errorMessage)
            : base()
        {
            Error = errorMessage;
        }
    }

    public class CommandTimeoutException : Exception
    {
        internal CommandTimeoutException(Exception innerException)
            : base("Command request timed out", innerException) { }
    }

    public class QueryCallException : Exception
    {
        public ErrorMessage Error { get; private init; }

        internal QueryCallException(ErrorMessage errorMessage)
            : base()
        {
            Error = errorMessage;
        }
    }
}
