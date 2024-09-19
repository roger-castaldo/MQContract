namespace MQContract.Interfaces
{
    /// <summary>
    /// Houses a set of metrics that were requested from the internal metric tracker.
    /// All message conversion durations are calculated from the perspective:
    ///     - When a class is being sent from the point of starting the middleware to the point where the class has been encoded into a service message and the middleware has completed
    ///     - When a service message is being recieved from the point of starting the middleware to the point where the class has been built from the service message and the middleware has completed
    /// </summary>
    public interface IContractMetric
    {
        /// <summary>
        /// Total number of messages
        /// </summary>
        ulong Messages { get; }
        /// <summary>
        /// Total amount of bytes from the messages
        /// </summary>
        ulong MessageBytes { get; }
        /// <summary>
        /// Average number of bytes from the messages
        /// </summary>
        ulong MessageBytesAverage { get; }
        /// <summary>
        /// Minimum number of bytes from the messages
        /// </summary>
        ulong MessageBytesMin { get; }
        /// <summary>
        /// Maximum number of bytes from the messages
        /// </summary>
        ulong MessageBytesMax { get; }
        /// <summary>
        /// Total time spent converting the messages
        /// </summary>
        TimeSpan MessageConversionDuration { get; }
        /// <summary>
        /// Average time to encode/decode the messages
        /// </summary>
        TimeSpan MessageConversionAverage { get; }
        /// <summary>
        /// Minimum time to encode/decode a message
        /// </summary>
        TimeSpan MessageConversionMin { get; }
        /// <summary>
        /// Maximum time to encode/decode a message
        /// </summary>
        TimeSpan MessageConversionMax { get; }
    }
}
