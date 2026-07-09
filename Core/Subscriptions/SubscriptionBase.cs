using Microsoft.Extensions.Logging;
using MQContract.Interfaces;
using MQContract.Interfaces.Service;
using MQContract.Logging;
using System.Diagnostics.CodeAnalysis;

namespace MQContract.Subscriptions;

internal abstract class SubscriptionBase<TMessage>(
    Func<string, ValueTask<string>> mapChannel, 
    MessageContext context, 
    string? channel, 
    bool synchronous, 
    ILogger logger,
    Action<Guid> remove) 
    : IInternalSubscription
{
    protected IServiceSubscription? serviceSubscription;
    private bool disposedValue;

    protected string MessageChannel { get; private init; } = Utility.GetChannel<TMessage>(mapChannel, context, channel);
    protected bool Synchronous { get; private init; } = synchronous;
    protected ILogger Logger => logger;
    protected IDisposable? SetScope() => logger?.BeginScope<string>($"Subscription[{ID}]");

    public Guid ID { get; private init; } = Guid.NewGuid();

    [ExcludeFromCodeCoverage(Justification = "Virtual function that is implemented elsewhere")]
    protected virtual void InternalDispose()
    { }

    ValueTask ISubscription.EndAsync()
        => EndAsync(true);
    ValueTask IInternalSubscription.EndAsyncWithoutRemoval()
        => EndAsync(false);

    private async ValueTask EndAsync(bool callRemove)
    {
        if (serviceSubscription!=null)
        {
            Logs.Lifetime.SubscriptionEndAsync(logger, ID);
            await serviceSubscription.EndAsync();
            serviceSubscription=null;
        }
        if (callRemove)
            remove(ID);
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
