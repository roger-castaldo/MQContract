namespace MQContract
{
    public interface IServiceSubscription : IDisposable
    {
        Task EndAsync();
    }
}
