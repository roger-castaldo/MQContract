namespace MQContract.Interfaces.Consumers
{
    /// <summary>
    /// Represents the Base for all Consumer interfaces and contains the common method definition
    /// </summary>
    public interface IBaseConsumer
    {
        /// <summary>
        /// Called when an error is received from within the underlying subscription that is using this Consumer
        /// </summary>
        /// <param name="error">The error that occured</param>
        void ErrorRecieved(Exception error);
    }
}
