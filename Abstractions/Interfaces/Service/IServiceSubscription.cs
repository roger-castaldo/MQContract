namespace MQContract.Interfaces.Service
{
    public interface IServiceSubscription : IDisposable
    {
        Task EndAsync();
    }
}
