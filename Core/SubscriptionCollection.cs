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
        : IDisposable
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

        public async Task RemoveAsync(Guid ID)
        {
            await dataLock.WaitAsync();
            subscriptions.RemoveAll(sub=>Equals(sub.ID, ID));
            dataLock.Release();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    dataLock.Wait();
                    Task.WaitAll(subscriptions.Select(sub => sub.EndAsync(false)).ToArray());
                    dataLock.Release();
                    dataLock.Dispose();
                    subscriptions.Clear();
                }
                disposedValue=true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
