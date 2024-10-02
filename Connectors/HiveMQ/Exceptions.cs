namespace MQContract.HiveMQ
{
    /// <summary>
    /// Thrown when the service connection is unable to connect to the HiveMQTT server
    /// </summary>
    public class ConnectionFailedException : Exception
    {
        internal ConnectionFailedException(string? reason)
            : base($"Failed to connect: {reason}")
        { }
    }
}
