using Microsoft.Extensions.Logging;
using MQContract.Factories;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Factories;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using MQContract.Middleware;

namespace MQContract
{
    /// <summary>
    /// The primary ContractConnection item which implements IContractConnection
    /// </summary>
    public sealed partial class ContractConnection
                : IContractConnection
    {
        private readonly Guid indentifier = Guid.NewGuid();
        private readonly SemaphoreSlim dataLock = new(1, 1);
        private readonly IMessageServiceConnection serviceConnection;
        private readonly IMessageEncoder? defaultMessageEncoder;
        private readonly IMessageEncryptor? defaultMessageEncryptor;
        private readonly IServiceProvider? serviceProvider;
        private readonly ILogger? logger;
        private readonly ChannelMapper? channelMapper;
        private readonly List<object> middleware;
        private readonly SemaphoreSlim inboxSemaphore = new(1, 1);
        private readonly Dictionary<Guid, TaskCompletionSource<ServiceQueryResult>> inboxResponses = [];
        private IServiceSubscription? inboxSubscription;
        private IEnumerable<IMessageTypeFactory> typeFactories = [];
        private bool disposedValue;

        private ContractConnection(IMessageServiceConnection serviceConnection,
        IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null)
        {
            this.serviceConnection = serviceConnection;
            this.defaultMessageEncoder = defaultMessageEncoder;
            this.defaultMessageEncryptor= defaultMessageEncryptor;
            this.serviceProvider = serviceProvider;
            this.logger=logger;
            this.channelMapper=channelMapper;
            this.middleware= [new ChannelMappingMiddleware(this.channelMapper)];
        }

        /// <summary>
        /// This is the call used to create an instance of a Contract Connection which will return the Interface
        /// </summary>
        /// <param name="serviceConnection">The service connection implementation to use for the underlying message requests.</param>
        /// <param name="defaultMessageEncoder">A default message encoder implementation if desired.  If there is no specific encoder for a given type, this encoder would be called.  The built in default being used dotnet Json serializer.</param>
        /// <param name="defaultMessageEncryptor">A default message encryptor implementation if desired.  If there is no specific encryptor </param>
        /// <param name="serviceProvider">A service prodivder instance supplied in the case that dependency injection might be necessary</param>
        /// <param name="logger">An instance of a logger if logging is desired</param>
        /// <param name="channelMapper">An instance of a ChannelMapper used to translate channels from one instance to another based on class channel attributes or supplied channels if necessary.
        /// For example, it might be necessary for a Nats.IO instance when you are trying to read from a stored message stream that is comprised of another channel or set of channels
        /// </param>
        /// <returns>An instance of IContractConnection</returns>
        public static IContractConnection Instance(IMessageServiceConnection serviceConnection,
        IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null)
            => new ContractConnection(serviceConnection,defaultMessageEncoder,defaultMessageEncryptor,serviceProvider,logger, channelMapper);

        private IMessageFactory<T> GetMessageFactory<T>(bool ignoreMessageHeader = false) where T : class
        {
            dataLock.Wait();
            var result = (IMessageFactory<T>?)typeFactories.FirstOrDefault(fact => fact.GetType().GetGenericArguments()[0]==typeof(T));
            dataLock.Release();
            if (result==null)
            {
                result = new MessageTypeFactory<T>(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, ignoreMessageHeader, serviceConnection.MaxMessageBodySize);
                dataLock.Wait();
                if (!typeFactories.Any(fact => fact.GetType().GetGenericArguments()[0]==typeof(T) && fact.IgnoreMessageHeader==ignoreMessageHeader))
                    typeFactories = typeFactories.Concat([(IMessageTypeFactory)result]);
                dataLock.Release();
            }
            return result;
        }

        private ValueTask<string> MapChannel(ChannelMapper.MapTypes mapType, string originalChannel)
            => channelMapper?.MapChannel(mapType, originalChannel)??ValueTask.FromResult<string>(originalChannel);

        ValueTask<PingResult> IContractConnection.PingAsync()
            => (serviceConnection is IPingableMessageServiceConnection pingableService ? pingableService.PingAsync() : throw new NotSupportedException("The underlying service does not support Ping"));

        async ValueTask IContractConnection.CloseAsync()
        {
            await (inboxSubscription?.EndAsync()??ValueTask.CompletedTask);
            await (serviceConnection?.CloseAsync()??ValueTask.CompletedTask);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (inboxSubscription is IDisposable subDisposable)
                        subDisposable.Dispose();
                    else if (inboxSubscription is IAsyncDisposable asyncSubDisposable)
                        asyncSubDisposable.DisposeAsync().AsTask().Wait();
                    if (serviceConnection is IDisposable disposable)
                        disposable.Dispose();
                    else if (serviceConnection is IAsyncDisposable asyncDisposable)
                        asyncDisposable.DisposeAsync().AsTask().Wait();
                }
                dataLock.Dispose();
                inboxSemaphore.Dispose();
                disposedValue=true;
            }
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (inboxSubscription is IAsyncDisposable asyncSubDisposable)
                asyncSubDisposable.DisposeAsync().AsTask().Wait();
            else if (inboxSubscription is IDisposable subDisposable)
                subDisposable.Dispose();
            if (serviceConnection is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(true);
            else if (serviceConnection is IDisposable disposable)
                disposable.Dispose();

            Dispose(false);
            GC.SuppressFinalize(this);
        }
    }
}
