using Grpc.Core;

namespace MQContract.KubeMQ
{
    /// <summary>
    /// Thrown when an error occurs attempting to connect to the KubeMQ server.  
    /// Specifically this will be thrown when the Ping that is executed on each initial connection fails.
    /// </summary>
    public class UnableToConnectException : Exception
    {
        internal UnableToConnectException()
            : base("Unable to establish connection to the KubeMQ host") { }
    }

    /// <summary>
    /// Thrown when a call is made to an underlying KubeClient after the client has been disposed
    /// </summary>
    public class ClientDisposedException : Exception
    {
        internal ClientDisposedException()
            : base("Client has already been disposed") { }
    }

    /// <summary>
    /// Thrown when an error occurs sending and rpc response
    /// </summary>
    public class MessageResponseTransmissionException : Exception
    {
        internal MessageResponseTransmissionException(Guid subscriptionID,string messageID, Exception error)
            : base($"An error occured attempting to transmit the message response on subscription {subscriptionID} to message id {messageID}", error) { }
    }

    internal class NullResponseException : NullReferenceException
    {
        internal NullResponseException()
            : base("null response recieved from KubeMQ server") { }
    }

    internal class RPCErrorException : Exception
    {
        internal RPCErrorException(RpcException error)
            : base($"Status: {error.Status}, Message: {error.Message}") { }
    }
}
