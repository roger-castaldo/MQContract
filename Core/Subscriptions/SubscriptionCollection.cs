using MQContract.Interfaces;

namespace MQContract.Subscriptions;

#pragma warning disable S3881 // "IDisposable" should be implemented correctly
internal class SubscriptionCollection(IEnumerable<ISubscription> subscriptions)
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
    : ISubscription
{
    private bool disposedValue;

    ValueTask IAsyncDisposable.DisposeAsync()
        => subscriptions.Select(s => s.DisposeAsync()).WhenAll();

    ValueTask ISubscription.EndAsync()
        => subscriptions.Select(s => s.EndAsync()).WhenAll();

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                foreach (var subscription in subscriptions)
                    subscription.Dispose();
            }
            disposedValue=true;
        }
    }

    void IDisposable.Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
