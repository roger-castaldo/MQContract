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
    /// <param name="channel">The channel to be used</param>
    /// <param name="typeName">The message type to use</param>
    /// <param name="typeVersion">The message version to use</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MessageAttribute(string? channel = null, string? typeName = null, string? typeVersion = null) : Attribute
    {
        /// <summary>
        /// The Channel specified
        /// </summary>
        public string? Channel => channel;

        /// <summary>
        /// The name of the message type used when transmitting
        /// </summary>
        public string? TypeName => typeName;

        /// <summary>
        /// The version number to tag this message with during transmission
        /// </summary>
        public Version TypeVersion => new(typeVersion??"0.0.0.0");
    }
}
