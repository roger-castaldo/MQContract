namespace MQContract.Attributes
{
    /// <summary>
    /// Use this attribute to specify the Channel, TypeName, TypeVersion, ResponseChannel, DefaultTimeout and/or ResponseType 
    /// of the given query call.
    /// IMPORTANT:  The response channel value should either be specified here or on a given query call when the underlying service connection does not support 
    /// either QueryResponse or Inbox style messaging
    /// </summary>
    /// <param name="channel">The channel to be used</param>
    /// <param name="typeName">The query type to use</param>
    /// <param name="typeVersion">The query type version to use</param>
    /// <param name="responseChannel">The responce channel to be used when an underlying service connection does not support QueryResponse or Inbox</param>
    /// <param name="responseTimeoutMilliseconds">The query response timeout to default to</param>
    /// <param name="responseType">The expected response type for the query</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class QueryMessageAttribute(string? channel = null, string? typeName = null, string? typeVersion = null,
        string? responseChannel = null, int responseTimeoutMilliseconds = 60*1000, Type? responseType = null)
        : MessageAttribute(channel, typeName, typeVersion)
    {
        /// <summary>
        /// The Response Channel defined for the given query
        /// </summary>
        public string? ResponseChannel => responseChannel;
        /// <summary>
        /// The Response Timeout defined for the given query
        /// </summary>
        public TimeSpan ResponseTimeout { get; private init; } = TimeSpan.FromMilliseconds(responseTimeoutMilliseconds);
        /// <summary>
        /// The Response Type defined for the given query
        /// </summary>
        public Type? ResponseType => responseType;
    }
}
