namespace MQContract.Interfaces.Middleware
{
    /// <summary>
    /// This is used to represent a Context for the middleware calls to use that exists from the start to the end of the message conversion process
    /// </summary>
    public interface IContext
    {
        /// <summary>
        /// Used to store and retreive values from the context during the conversion process.
        /// </summary>
        /// <param name="key">The unique key to use</param>
        /// <returns>The value if it exists in the context</returns>
        object? this[string key] { get; set; }
    }
}
