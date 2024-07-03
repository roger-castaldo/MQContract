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
        /// The Server Start Time of the host that was pinged
        /// </summary>
        DateTime ServerStartTime { get; }
        /// <summary>
        /// The Server Up Time of the host that was pinged
        /// </summary>
        TimeSpan ServerUpTime { get; }
    }
}
