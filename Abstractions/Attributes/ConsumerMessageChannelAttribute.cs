namespace MQContract.Attributes
{
    /// <summary>
    /// Use this attribute to specify the Channel name used for receiving messages by this consumer class.
    /// This would technically override the channel set by the Message class defined for the consumer,
    /// and can be overriden by passing a channel value when registering the consumer.
    /// </summary>
    /// <param name="name">The name of the Channel to be used for receving messages to this consumer</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ConsumerMessageChannelAttribute(string name) : Attribute
    {
        /// <summary>
        /// The name of the channel specified for this Consumer to listen to
        /// </summary>
        public string Name => name;
    }
}
