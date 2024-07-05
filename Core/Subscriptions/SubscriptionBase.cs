using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Service;
using System.Reflection;

namespace MQContract.Subscriptions
{
    internal abstract class SubscriptionBase<T>(string? channel=null,bool synchronous=false) : ISubscription
        where T : class
    {
        protected IServiceSubscription? serviceSubscription;
        protected readonly CancellationTokenSource token = new();
        protected readonly string MessageChannel = channel??typeof(T).GetCustomAttribute<MessageChannelAttribute>(false)?.Name??throw new MessageChannelNullException();
        protected readonly bool Synchronous = synchronous;
        private bool disposedValue;

        protected void SyncToken(CancellationToken cancellationToken)
            => cancellationToken.Register(() => token.Cancel());

        protected virtual void InternalDispose()
        { }

        public async Task EndAsync()
        {
            if (serviceSubscription!=null)
                await serviceSubscription.EndAsync();
            await token.CancelAsync();
        }

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    token.Cancel();
                    InternalDispose();
                    serviceSubscription?.Dispose();
                    token.Dispose();
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
