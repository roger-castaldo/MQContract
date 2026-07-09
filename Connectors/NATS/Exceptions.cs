namespace MQContract.NATS;

/// <summary>
/// Thrown when an error occurs attempting to connect to the NATS server.  
/// Specifically this will be thrown when the Ping that is executed on each initial connection fails.
/// </summary>
public class UnableToConnectException : Exception
{
    internal UnableToConnectException()
        : base("Unable to establish connection to the NATS host") { }
}

/// <summary>
/// Thrown when a query response error is recieved through the system
/// </summary>
public class QueryAsyncReponseException : Exception
{
    internal QueryAsyncReponseException(string error)
        : base(error) { }
}
