using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Service;
using MQContract.Interfaces.Subscriptions;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MQContract.Subscriptions
{
    internal abstract class SubscriptionBase<T> : IInternalSubscription
        where T : class
    {
        protected IServiceSubscription? serviceSubscription;
        private readonly SubscriptionCollection collection;
        protected readonly CancellationTokenSource token = new();
        private bool disposedValue;
        protected string MessageChannel { get; private init; }
        protected bool Synchronous { get; private init; }

        public Guid ID { get; private init; }

        protected SubscriptionBase(Func<string, Task<string>> mapChannel, SubscriptionCollection collection, string? channel=null,bool synchronous = false){
            ID = Guid.NewGuid();
            this.collection=collection;
            var chan = channel??typeof(T).GetCustomAttribute<MessageChannelAttribute>(false)?.Name??throw new MessageChannelNullException();
            Synchronous = synchronous;
            var tsk = mapChannel(chan);
            tsk.Wait();
            MessageChannel=tsk.Result;
        }

        protected void SyncToken(CancellationToken cancellationToken)
            => cancellationToken.Register(async () => await EndAsync());

        [ExcludeFromCodeCoverage(Justification ="Virtual function that is implemented elsewhere")]
        protected virtual void InternalDispose()
        { }

        public async ValueTask EndAsync(bool remove)
        {
            if (serviceSubscription!=null)
                await serviceSubscription.EndAsync();
            await token.CancelAsync();
            if (remove)
                await collection.RemoveAsync(ID);
        }

        public ValueTask EndAsync()
            => EndAsync(true);

        public async ValueTask DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                await EndAsync();
                InternalDispose();
                if (serviceSubscription!=null)
                    await serviceSubscription!.EndAsync();
                token.Dispose();
            }
        }
    }
}
