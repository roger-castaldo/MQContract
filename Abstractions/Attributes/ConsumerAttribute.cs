namespace MQContract.Attributes
{
    /// <summary>
    /// Use this attribute to define the Channel, Group and/or IngoreMessageTypeHeader flag
    /// for a given Consumer
    /// </summary>
    /// <param name="channel">The channel the consumer will listen on</param>
    /// <param name="group">The group the consumer will register as</param>
    /// <param name="ignoreMessageTypeHeader">A falg to indicate if ignoring the message type is desired</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ConsumerAttribute(string? channel=null,string? group=null,bool ignoreMessageTypeHeader=false) : Attribute
    {
        /// <summary>
        /// The channel to register the consumer on
        /// </summary>
        public string? Channel => channel;
        
        /// <summary>
        /// The group to register the consumer to
        /// </summary>
        public string? Group => group;

        /// <summary>
        /// Indicates if the message type should be ignored
        /// </summary>
        public bool IgnoreMessageTypeHeader => ignoreMessageTypeHeader;
    }
}
