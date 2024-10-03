namespace MQContract.Interfaces.Service
{
    /// <summary>
    /// Represents an underlying service level subscription
    /// </summary>
    public interface IServiceSubscription
    {
        /// <summary>
        /// Called to end the subscription
        /// </summary>
        /// <returns>A task to allow for asynchronous ending of the subscription</returns>
        ValueTask EndAsync();
    }
}
