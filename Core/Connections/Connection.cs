using Microsoft.Extensions.Logging;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Connections
{
    internal partial class Connection(IMessageServiceConnection serviceConnection,
        IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null) : 
        AConnection<IContractConnection>(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, logger, channelMapper),
        IContractConnection
    {
        private readonly SemaphoreSlim publishLock = new(1, 1);

        ValueTask<PingResult> IContractConnection.PingAsync()
            => (serviceConnection is IPingableMessageServiceConnection pingableService ? pingableService.PingAsync() : throw new NotSupportedException("The underlying service does not support Ping"));

        protected override async ValueTask CloseAsync()
        {
            await (serviceConnection?.CloseAsync()??ValueTask.CompletedTask);
        }

        protected override void InternalDispose()
        {
            if (serviceConnection is IDisposable disposable)
                disposable.Dispose();
            else if (serviceConnection is IAsyncDisposable asyncDisposable)
                asyncDisposable.DisposeAsync().AsTask().Wait();
            publishLock.Dispose();
        }

        protected override async ValueTask InternalDisposeAsync()
        {
            if (serviceConnection is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(true);
            else if (serviceConnection is IDisposable disposable)
                disposable.Dispose();
            publishLock.Dispose();
        }
    }
}
