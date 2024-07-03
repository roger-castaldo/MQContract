namespace MQContract.Attributes
{
    /// <summary>
    /// Use this attribute to specify the Channel name used for transmitting this message class.
    /// This can be overidden by specifying the channel on the method calls, but a value must 
    /// be specified, either using the attribute or by specifying in the input.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="name">The name of the Channel to be used for transmitting this message class.</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MessageChannelAttribute(string name) : Attribute
    {
        /// <summary>
        /// The name of the channel specified
        /// </summary>
        public string Name => name;
    }
}
