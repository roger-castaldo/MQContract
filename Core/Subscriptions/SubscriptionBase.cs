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
        private bool disposedValue;

        protected string MessageChannel { get; private init; }
        protected bool Synchronous { get; private init; }

        public Guid ID { get; private init; }

        protected SubscriptionBase(Func<string, ValueTask<string>> mapChannel, string? channel=null,bool synchronous = false){
            ID = Guid.NewGuid();
            var chan = channel??typeof(T).GetCustomAttribute<MessageChannelAttribute>(false)?.Name??throw new MessageChannelNullException();
            Synchronous = synchronous;
            var tsk = mapChannel(chan).AsTask();
            tsk.Wait();
            MessageChannel=tsk.Result;
        }

        [ExcludeFromCodeCoverage(Justification = "Virtual function that is implemented elsewhere")]
        protected virtual void InternalDispose()
        {  }

        public async ValueTask EndAsync()
        {
            if (serviceSubscription!=null)
            {
                System.Diagnostics.Debug.WriteLine("Calling subscription end async...");
                await serviceSubscription.EndAsync();
                System.Diagnostics.Debug.WriteLine("Subscription ended async");
                serviceSubscription=null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && serviceSubscription is IDisposable disposable)
                    disposable.Dispose();
                InternalDispose();
                disposedValue=true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (serviceSubscription is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(true);
            else if (serviceSubscription is IDisposable disposable)
                disposable.Dispose();

            Dispose(false);
            GC.SuppressFinalize(this);
        }
    }
}
