namespace MQContract.Interfaces.Service
{
    /// <summary>
    /// Represents an underlying service level subscription
    /// </summary>
    public interface IServiceSubscription : IAsyncDisposable
    {
        /// <summary>
        /// Called to end the subscription
        /// </summary>
        /// <returns>A task to allow for asynchronous ending of the subscription</returns>
        Task EndAsync();
    }
}
