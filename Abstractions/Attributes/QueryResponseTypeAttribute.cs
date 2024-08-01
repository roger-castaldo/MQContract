namespace MQContract.Attributes
{
    /// <summary>
    /// Used to allow the specification of a response type without supplying it to the contract calls
    /// </summary>
    /// <remarks>
    /// Default constructor
    /// </remarks>
    /// <param name="responseType">The type of class that should be expected for a response</param>
    [AttributeUsage(AttributeTargets.Class)]
    public class QueryResponseTypeAttribute(Type responseType) : Attribute
    {
        /// <summary>
        /// The type of class that should be expected for a Response when not specified
        /// </summary>
        public Type ResponseType => responseType;
    }
}
