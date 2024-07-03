namespace MQContract.Attributes
{
    /// <summary>
    /// Used to specify the name of the message type inside the system.  
    /// Default is to use the class name, however, this can be used to 
    /// override that and allow for different versions of a message to 
    /// have the same name withing the tranmission system.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="value">The name to use for the class when transmitting</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MessageNameAttribute(string value)
                : Attribute
    {
        public string Value => value;
    }
}
