using MQContract.Interfaces;
using MQContract.Interfaces.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract
{
    internal class SubscriptionCollection
        : IAsyncDisposable
    {
        private readonly SemaphoreSlim dataLock = new(1, 1);
        private readonly List<IInternalSubscription> subscriptions = [];
        private bool disposedValue;

        public async Task AddAsync(IInternalSubscription subscription)
        {
            await dataLock.WaitAsync();
            subscriptions.Add(subscription);
            dataLock.Release();
        }

        public async ValueTask DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue = true;
                await dataLock.WaitAsync();
                await Task.WhenAll(subscriptions.Select(sub => sub.EndAsync(false).AsTask()));
                dataLock.Release();
                dataLock.Dispose();
                subscriptions.Clear();
            }
        }

        public async Task RemoveAsync(Guid ID)
        {
            await dataLock.WaitAsync();
            subscriptions.RemoveAll(sub=>Equals(sub.ID, ID));
            dataLock.Release();
        }
    }
}
