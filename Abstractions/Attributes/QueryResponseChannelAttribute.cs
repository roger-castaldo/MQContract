namespace MQContract.Attributes
{
    /// <summary>
    /// Used to allow the specification of a response channel to be used without supplying it to the contract calls.  
    /// IMPORTANT:  This particular attribute and the response channel argument are only used when the underlying connection does not support QueryResponse messaging.
    /// </summary>
    /// <param name="name">The name of the channel to use for responses</param>
    [AttributeUsage(AttributeTargets.Class)]
    public class QueryResponseChannelAttribute(string name) : Attribute
    {
        /// <summary>
        /// The Name of the response channel
        /// </summary>
        public string Name => name;
    }
}
