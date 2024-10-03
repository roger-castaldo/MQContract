namespace MQContract.Attributes
{
    /// <summary>
    /// Use this attribute to specify the timeout (in milliseconds) for a response 
    /// from an RPC call for the specific class that this is attached to.  This can 
    /// be overridden by supplying a timeout value when making an RPC call.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="value">The number of milliseconds for an RPC call response to return</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MessageResponseTimeoutAttribute(int value) : Attribute
    {
        /// <summary>
        /// The number of milliseconds for the timeout to trigger for this RPC call class
        /// </summary>
        public int Value => value;

        /// <summary>
        /// The converted TimeSpan value from the supplied milliseconds value in the constructor
        /// </summary>
        public TimeSpan TimeSpanValue => TimeSpan.FromMilliseconds(value);
    }
}
