using Microsoft.Extensions.Logging;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static MQContract.Connections.ServiceConnectionList;

namespace MQContract.Connections
{
    internal abstract class AMappableConnection<CC>(IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null)
        : AConnection<CC>(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, logger, channelMapper),
        IMappableContractConnection<CC>
        where CC : IBaseContractConnection
    {
        private readonly ServiceConnectionList connectionList = new();

        protected IEnumerable<ServiceConnection> FullList => connectionList.FullList;
        protected uint? MaxMessageBodySize => connectionList.MaxMessageBodySize;

        protected CC RegisterServiceConnection(Func<(string channel, Type messageType, MessageHeader messageHeader), bool> checkCallback, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
        {
            connectionList.Add(checkCallback, serviceConnectionName, messageServiceConnection);
            return (CC)(IBaseContractConnection)this;
        }

        CC IMappableContractConnection<CC>.RegisterServiceConnection(Func<(string channel, Type messageType, MessageHeader messageHeader), bool> checkCallback, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            => RegisterServiceConnection(pars => checkCallback(pars), serviceConnectionName, messageServiceConnection);

        CC IMappableContractConnection<CC>.RegisterServiceConnection(string channel, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            => RegisterServiceConnection(pars => Equals(pars.channel, channel), serviceConnectionName, messageServiceConnection);

        CC IMappableContractConnection<CC>.RegisterServiceConnection(Type messageType, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            => RegisterServiceConnection(pars => Equals(pars.messageType, messageType), serviceConnectionName, messageServiceConnection);

        CC IMappableContractConnection<CC>.RegisterServiceConnection(string messageHeaderKey, string messageHeaderValue, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            => RegisterServiceConnection(pars => Equals(pars.messageHeader[messageHeaderKey], messageHeaderValue), serviceConnectionName, messageServiceConnection);

        protected async ValueTask<IEnumerable<ServiceConnectionList.ServiceConnection>> GetConnectionsAsync(string channel, Type messageType, MessageHeader messageHeader)
        {
            var result = await connectionList.GetAsync(channel, messageType, messageHeader);
            if (!result.Any())
                throw new KeyNotFoundException("Unable to locate an underlying service connection to use for the given conditions.");
            return result;
        }

        protected async ValueTask<(IEnumerable<ServiceConnectionList.ServiceConnection> connections,string channel)> GetConnectionsAsync<T>(string? channel, ChannelMapper.MapTypes mapTypes)
            where T : class
        {
            channel = await Utility.GetChannelAsync<T>((originalChannel) => MapChannel(mapTypes, originalChannel), channel);
            return (await GetConnectionsAsync(channel, typeof(T), new MessageHeader([])),channel);
        }

        protected sealed override async ValueTask CloseAsync()
            => await connectionList.CloseAsync();

        protected override void InternalDispose()
            => connectionList.Dispose();
    }
}
