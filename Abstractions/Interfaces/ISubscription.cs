namespace MQContract.Interfaces
{
    public interface ISubscription : IDisposable
    {
        Task EndAsync();
    }
}
