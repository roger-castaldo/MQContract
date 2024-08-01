namespace MQContract.Interfaces
{
    /// <summary>
    /// This interface represents a Contract Connection Subscription and is used to house and end the subscription
    /// </summary>
    public interface ISubscription : IDisposable
    {
        /// <summary>
        /// Called to end (close off) the subscription
        /// </summary>
        /// <returns>A task that is ending the subscription and closing off the resources for it</returns>
        Task EndAsync();
    }
}
