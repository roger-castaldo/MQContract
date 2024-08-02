namespace MQContract.KubeMQ.Interfaces
{
    /// <summary>
    /// The definition for a PingResponse coming from KubeMQ that has a couple of extra properties available
    /// </summary>
    public interface IKubeMQPingResult
    {
        /// <summary>
        /// The host name for the server pinged
        /// </summary>
        string Host { get; }
        /// <summary>
        /// The current version of KubeMQ running on it
        /// </summary>
        string Version { get; } 
        /// <summary>
        /// How long it took the server to respond to the request
        /// </summary>
        TimeSpan ResponseTime { get; }
        /// <summary>
        /// The Server Start Time of the host that was pinged
        /// </summary>
        DateTime ServerStartTime { get; }
        /// <summary>
        /// The Server Up Time of the host that was pinged
        /// </summary>
        TimeSpan ServerUpTime { get; }
    }
}
