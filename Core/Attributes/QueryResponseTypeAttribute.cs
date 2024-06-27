namespace MQContract.Attributes
{
    /// <summary>
    /// Used to lock an RPC Query request message to a specific response type
    /// </summary>
    /// <remarks>
    /// Default constructor
    /// </remarks>
    /// <param name="responseType">The type of class that should be expected for a response</param>
    [AttributeUsage(AttributeTargets.Class)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "The type of operation is RPC")]
    public class QueryResponseTypeAttribute(Type responseType) : Attribute
    {
        public Type ResponseType => responseType;
    }
}
