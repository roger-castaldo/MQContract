using Microsoft.Extensions.Logging;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Diagnostics.Metrics;

namespace MQContract.Connections
{
    internal partial class Connection(IMessageServiceConnection serviceConnection,
        IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null) : AConnection(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, logger, channelMapper),
        IContractConnection
    {
        private readonly SemaphoreSlim publishLock = new(1, 1);
        private readonly SemaphoreSlim inboxSemaphore = new(1, 1);
        private readonly Dictionary<Guid, TaskCompletionSource<ServiceQueryResult>> inboxResponses = [];
        private IServiceSubscription? inboxSubscription;

        IContractConnection IContractConnection.AddMetrics(Meter? meter, bool useInternal)
            => (Connection)AddMetrics(meter, useInternal);
        
        async ValueTask IContractConnection.CloseAsync()
        {
            await (inboxSubscription?.EndAsync()??ValueTask.CompletedTask);
            await (serviceConnection?.CloseAsync()??ValueTask.CompletedTask);
        }

        IContractMetric? IContractConnection.GetSnapshot(bool sent)
            => GetSnapshot(sent);

        IContractMetric? IContractConnection.GetSnapshot(Type messageType, bool sent)
            => GetSnapshot(messageType, sent);

        IContractMetric? IContractConnection.GetSnapshot<T>(bool sent)
            => GetSnapshot<T>(sent);

        IContractMetric? IContractConnection.GetSnapshot(string channel, bool sent)
            => GetSnapshot(channel, sent);  

        ValueTask<PingResult> IContractConnection.PingAsync()
            => (serviceConnection is IPingableMessageServiceConnection pingableService ? pingableService.PingAsync() : throw new NotSupportedException("The underlying service does not support Ping"));

        IContractConnection IContractConnection.RegisterMiddleware<T>()
            => (Connection)RegisterMiddleware<T>();

        IContractConnection IContractConnection.RegisterMiddleware<T>(Func<T> constructInstance)
            => (Connection)RegisterMiddleware<T>(constructInstance);

        IContractConnection IContractConnection.RegisterMiddleware<T, M>()
            => (Connection)RegisterMiddleware<T, M>();

        IContractConnection IContractConnection.RegisterMiddleware<T, M>(Func<T> constructInstance)
            => (Connection)RegisterMiddleware<T, M>(constructInstance);

        protected override void InternalDispose()
        {
            if (inboxSubscription is IDisposable subDisposable)
                subDisposable.Dispose();
            else if (inboxSubscription is IAsyncDisposable asyncSubDisposable)
                asyncSubDisposable.DisposeAsync().AsTask().Wait();
            if (serviceConnection is IDisposable disposable)
                disposable.Dispose();
            else if (serviceConnection is IAsyncDisposable asyncDisposable)
                asyncDisposable.DisposeAsync().AsTask().Wait();
            inboxSemaphore.Dispose();
            publishLock.Dispose();
        }

        protected override async ValueTask InternalDisposeAsync()
        {
            if (inboxSubscription is IAsyncDisposable asyncSubDisposable)
                asyncSubDisposable.DisposeAsync().AsTask().Wait();
            else if (inboxSubscription is IDisposable subDisposable)
                subDisposable.Dispose();
            if (serviceConnection is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(true);
            else if (serviceConnection is IDisposable disposable)
                disposable.Dispose();
            inboxSemaphore.Dispose();
            publishLock.Dispose();
        }
    }
}
