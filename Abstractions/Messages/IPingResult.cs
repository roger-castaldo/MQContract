namespace MQContract.Messages
{
    /// <summary>
    /// Represents a Ping Result from the underlying service
    /// </summary>
    public interface IPingResult
    {
        /// <summary>
        /// The name/IP of the host that was pinged
        /// </summary>
        string Host { get; }
        /// <summary>
        /// The version string of the host that was pinged
        /// </summary>
        string Version { get; }
        /// <summary>
        /// How long it took for the server to respond to the Ping request
        /// </summary>
        TimeSpan ResponseTime { get; }
    }
}
