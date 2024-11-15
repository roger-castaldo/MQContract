using Microsoft.Extensions.Logging;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Connections
{
    internal partial class MultiServiceConnection(IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null) :
        AConnection<IMultiServiceContractConnection>(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, logger, channelMapper),
        IMultiServiceContractConnection
    {
        private readonly SemaphoreSlim publishLock = new(1, 1);
        private readonly ServiceConnectionList connectionList = new();

        private MultiServiceConnection RegisterServiceConnection(Func<(string channel, Type messageType, MessageHeader messageHeader), bool> checkCallback, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
        {
            connectionList.Add(checkCallback, serviceConnectionName, messageServiceConnection);
            return this;
        }

        IMultiServiceContractConnection IMultiServiceContractConnection.RegisterServiceConnection(string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            => RegisterServiceConnection(pars=>true, serviceConnectionName, messageServiceConnection);

        IMultiServiceContractConnection IMultiServiceContractConnection.RegisterServiceConnection(Func<(string channel, Type messageType, MessageHeader messageHeader), bool> checkCallback, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            => RegisterServiceConnection(pars => checkCallback(pars), serviceConnectionName, messageServiceConnection);

        IMultiServiceContractConnection IMultiServiceContractConnection.RegisterServiceConnection(string channel, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            => RegisterServiceConnection(pars => Equals(pars.channel,channel), serviceConnectionName, messageServiceConnection);

        IMultiServiceContractConnection IMultiServiceContractConnection.RegisterServiceConnection(Type messageType, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            => RegisterServiceConnection(pars => Equals(pars.messageType,messageType), serviceConnectionName, messageServiceConnection);

        IMultiServiceContractConnection IMultiServiceContractConnection.RegisterServiceConnection(string messageHeaderKey, string messageHeaderValue, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            => RegisterServiceConnection(pars => Equals(pars.messageHeader[messageHeaderKey],messageHeaderValue), serviceConnectionName, messageServiceConnection);

        private async ValueTask<IEnumerable<ServiceConnectionList.ServiceConnection>> GetConnectionsAsync(string channel, Type messageType, MessageHeader messageHeader)
        {
            var result = await connectionList.GetAsync(channel, messageType, messageHeader);
            if (!result.Any())
                throw new KeyNotFoundException("Unable to locate an underlying service connection to use for the given conditions.");
            return result;
        }

        async ValueTask<IEnumerable<PingResult>> IMultiServiceContractConnection.PingAsync()
            => await connectionList.FullList
                .Select(ss => ss.MessageServiceConnection)
                .OfType<IPingableMessageServiceConnection>()
                .WhenAll(pmc => pmc.PingAsync());

        protected override async ValueTask CloseAsync()
            => await connectionList.CloseAsync();

        protected override void InternalDispose()
        {
            connectionList.Dispose();
            publishLock.Dispose();
        }

        protected override ValueTask InternalDisposeAsync()
        {
            InternalDispose();
            return ValueTask.CompletedTask;
        }
    }
}
