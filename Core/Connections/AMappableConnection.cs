using Microsoft.Extensions.Logging;
using MQContract.Extensions;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Service;
using MQContract.Messages;

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


        protected IEnumerable<ServiceConnectionList.ServiceConnection> FullList => connectionList.FullList;
        protected uint? MaxMessageBodySize => connectionList.MaxMessageBodySize;

        protected CC RegisterServiceConnection(Func<(string channel, Type messageType, MessageHeader messageHeader), bool> checkCallback, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
        {
            connectionList.Add(checkCallback, serviceConnectionName, messageServiceConnection);
            return (CC)(IBaseContractConnection)this;
        }

        protected override ConnectionHealthCheck? ProduceConnectionHealthCheck()
            => new(serviceConnectionList: connectionList);

        CC IMappableContractConnection<CC>.RegisterServiceConnection(Func<(string channel, Type messageType, MessageHeader messageHeader), bool> checkCallback, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            => RegisterServiceConnection(pars => checkCallback(pars), serviceConnectionName, messageServiceConnection);

        CC IMappableContractConnection<CC>.RegisterServiceConnection(string channel, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            => RegisterServiceConnection(pars => Equals(pars.channel, channel), serviceConnectionName, messageServiceConnection);

        CC IMappableContractConnection<CC>.RegisterServiceConnection(Type messageType, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            => RegisterServiceConnection(pars => Equals(pars.messageType, messageType), serviceConnectionName, messageServiceConnection);

        CC IMappableContractConnection<CC>.RegisterServiceConnection<TMessage>(string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            => RegisterServiceConnection(pars => Equals(pars.messageType, typeof(TMessage)), serviceConnectionName, messageServiceConnection);

        CC IMappableContractConnection<CC>.RegisterServiceConnection(string messageHeaderKey, string messageHeaderValue, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            => RegisterServiceConnection(pars => Equals(pars.messageHeader[messageHeaderKey], messageHeaderValue), serviceConnectionName, messageServiceConnection);

        CC IMappableContractConnection<CC>.RegisterResiliencePolicy(string serviceConnectionName, (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => AddPolicy(serviceConnectionName, null, retryPolicy, circuitBreakPolicy);

        CC IMappableContractConnection<CC>.RegisterResiliencePolicy<TMessage>(string serviceConnectionName, (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => AddPolicy(serviceConnectionName, typeof(TMessage), retryPolicy, circuitBreakPolicy);

        CC IMappableContractConnection<CC>.RegisterResiliencePolicy(string serviceConnectionName, Type messageType, (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => AddPolicy(serviceConnectionName, messageType, retryPolicy, circuitBreakPolicy);

        CC IMappableContractConnection<CC>.RegisterResiliencePolicy(string serviceConnectionName, string messageChannel, (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => AddPolicy(serviceConnectionName, messageChannel, retryPolicy, circuitBreakPolicy);

        protected async ValueTask<IEnumerable<ServiceConnectionList.ServiceConnection>> GetConnectionsAsync(string channel, Type messageType, MessageHeader messageHeader)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Locating connection(s) for {Channel}, {MessageType}, {HeaderKeys}", channel, messageType, string.Join(',', messageHeader.Keys));
            var result = await connectionList.GetAsync(channel, messageType, messageHeader);
            if (!result.Any())
            {
                Logger?.LogErrorChecked("Unable to locate any connections matching {Channel}, {MessageType}, {HeaderKeys}", channel, messageType, string.Join(',', messageHeader.Keys));
                throw new NoConnectionMatchException();
            }
            return result;
        }

        protected async ValueTask<(IEnumerable<ServiceConnectionList.ServiceConnection> connections, string channel)> GetConnectionsAsync<TMessage>(string? channel, ChannelMapper.MapTypes mapTypes)
        {
            channel = await Utility.GetChannelAsync<TMessage>((originalChannel) => MapChannel(mapTypes, originalChannel), channel);
            return (await GetConnectionsAsync(channel, typeof(TMessage), new MessageHeader([])), channel);
        }

        protected sealed override async ValueTask CloseAsync()
            => await connectionList.CloseAsync();

        protected override async ValueTask InternalDisposeAsync()
            => await ((IAsyncDisposable)connectionList).DisposeAsync();
    }
}
