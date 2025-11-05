namespace MQContract.Attributes
{
    /// <summary>
    /// Used to allow the specification of a response channel to be used without supplying it to the contract calls.  
    /// IMPORTANT:  This particular attribute and the response channel argument are only used when the underlying connection does not support QueryResponse messaging.
    /// </summary>
    /// <param name="name">The name of the channel to use for responses</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class QueryMessageAttribute(string? channel = null, string? typeName = null, string? typeVersion = null,
        string? responseChannel=null,int responseTimeoutMilliseconds=60*1000,Type? responseType=null) 
        : MessageAttribute(channel,typeName,typeVersion)
    {
        public string? ResponseChannel => responseChannel;
        public TimeSpan ResponseTimeout { get; private init; } = TimeSpan.FromMilliseconds(responseTimeoutMilliseconds);
        public Type? ResponseType => responseType;
    }
}
