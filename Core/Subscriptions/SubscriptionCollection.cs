using MQContract.Interfaces;

namespace MQContract.Subscriptions
{
    internal class SubscriptionCollection(IEnumerable<ISubscription> subscriptions)
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
                    foreach(var subscription in subscriptions)
                        subscription.Dispose();
                }
                disposedValue=true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SubscriptionCollection()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
