using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Service;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MQContract.Subscriptions
{
    internal abstract class SubscriptionBase<T> : ISubscription
        where T : class
    {
        protected IServiceSubscription? serviceSubscription;
        protected readonly CancellationTokenSource token = new();
        private bool disposedValue;
        protected string MessageChannel { get; private init; }
        protected bool Synchronous { get; private init; }

        protected SubscriptionBase(Func<string, Task<string>> mapChannel, string? channel=null,bool synchronous = false){
            var chan = channel??typeof(T).GetCustomAttribute<MessageChannelAttribute>(false)?.Name??throw new MessageChannelNullException();
            Synchronous = synchronous;
            var tsk = mapChannel(chan);
            tsk.Wait();
            MessageChannel=tsk.Result;
        }

        protected void SyncToken(CancellationToken cancellationToken)
            => cancellationToken.Register(() => EndAsync().Wait());

        [ExcludeFromCodeCoverage(Justification ="Virtual function that is implemented elsewhere")]
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
                    EndAsync().Wait();
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
