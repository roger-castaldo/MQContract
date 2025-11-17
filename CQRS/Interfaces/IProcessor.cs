namespace MQContract.CQRS.Interfaces
{
    /// <summary>
    /// The base interface housing common calls for a Processor
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// Called when an error is supplied from the underlying Contract Connection
        /// </summary>
        /// <param name="error">The error that occured</param>
        void ErrorRecieved(Exception error);
    }
}
