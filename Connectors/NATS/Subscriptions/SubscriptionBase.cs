using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MQContract.NATS.Subscriptions
{
    internal abstract class SubscriptionBase(CancellationToken cancellationToken) : IInternalServiceSubscription
    {
        private bool disposedValue;
        protected readonly CancellationTokenSource cancelToken = new();

        protected abstract Task RunAction();
        public void Run()
        {
            cancellationToken.Register(() =>
            {
                cancelToken.Cancel();
            });
            RunAction();
        }

        public async Task EndAsync()
        {
            try { await cancelToken.CancelAsync(); } catch { }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cancelToken.Cancel();
                    cancelToken.Dispose();
                }
                disposedValue=true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
