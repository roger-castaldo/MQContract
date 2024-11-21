using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Reflection;

namespace MQContract.Connections
{
    internal class MappedConnection(IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null) :
        AMappableConnection<IMappedContractConnection>(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, logger, channelMapper),
        IMappedContractConnection
    {
        private readonly SemaphoreSlim publishLock = new(1, 1);
        ValueTask<PingResult> IContractConnection.PingAsync()
        {
            var connections = FullList.Select(c=>c.MessageServiceConnection).OfType<IPingableMessageServiceConnection>();
            return connections.Count() switch
            {
                0 => throw new PingNotSupportedException(),
                1 => connections.First().PingAsync(),
                _ => throw new TooManyConnectionMatchesException()
            };
        }

        protected override void InternalDispose()
        {
            base.InternalDispose();
            publishLock.Dispose();
        }

        protected override async ValueTask InternalDisposeAsync()
        {
            await base.InternalDisposeAsync();
            publishLock.Dispose();
        }

        private new async ValueTask<ServiceConnectionList.ServiceConnection> GetConnectionsAsync(string channel, Type messageType, MessageHeader messageHeader)
        {
            var connections = await base.GetConnectionsAsync(channel, messageType, messageHeader);
            if (connections.Count()>1) throw new TooManyConnectionMatchesException();
            return connections.First();
        }

        private new async ValueTask<(ServiceConnectionList.ServiceConnection connections, string channel)> GetConnectionsAsync<T>(string? channel, ChannelMapper.MapTypes mapTypes)
            where T : class
        {
            (var connections,channel) = await base.GetConnectionsAsync<T>(channel, mapTypes);
            if (connections.Count()>1) throw new TooManyConnectionMatchesException();
            return (connections.First(),channel);
        }

        #region PubSub
        protected override async ValueTask<ISubscription> CreateSubscriptionAsync<T>(Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, CancellationToken cancellationToken)
            where T : class
        {
            (var connection, channel) = await GetConnectionsAsync<T>(channel, ChannelMapper.MapTypes.PublishSubscription);
            return await CreateSubscriptionAsync<T>(
                GetMessageFactory<T>(connection.MessageServiceConnection.MaxMessageBodySize, ignoreMessageHeader),
                connection.MessageServiceConnection,
                messageReceived,
                errorReceived,
                channel,
                group,
                synchronous,
                cancellationToken
            );
        }

        async ValueTask<TransmissionResult> IContractConnection.PublishAsync<T>(T message, string? channel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            var serviceMessage = await ProduceServiceMessageAsync<T>(ChannelMapper.MapTypes.Publish, GetMessageFactory<T>(MaxMessageBodySize), message, false, channel, messageHeader);
            var serviceConnection = await GetConnectionsAsync(serviceMessage.Channel, typeof(T), serviceMessage.Header);
            await publishLock.WaitAsync(cancellationToken);
            var result = await serviceConnection.MessageServiceConnection.PublishAsync(
                serviceMessage,
                cancellationToken
            );
            publishLock.Release();
            return result;
        }

        async ValueTask<IEnumerable<TransmissionResult>> IContractConnection.BulkPublishAsync<T>(IEnumerable<(T message, MessageHeader? messageHeader)> messages, string? channel, CancellationToken cancellationToken)
        {
            var serviceMessages = await
            messages.WhenAll(m =>
                    ProduceServiceMessageAsync<T>(ChannelMapper.MapTypes.Publish, GetMessageFactory<T>(MaxMessageBodySize), m.message, false, channel, m.messageHeader)
                );
            var serviceConnection = await GetConnectionsAsync(serviceMessages.First().Channel, typeof(T), serviceMessages.First().Header);
            await publishLock.WaitAsync(cancellationToken);
            var result = await BulkPublishAsync(serviceMessages, serviceConnection.MessageServiceConnection, cancellationToken);
            publishLock.Release();
            return result;
        }
        #endregion

        #region QueryResponse
        private async ValueTask<QueryResult<R>> ProcessQueryAsync<Q, R>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
            where Q : class
            where R : class
        {
            var serviceMessage = await ProduceServiceMessageAsync<Q>(ChannelMapper.MapTypes.Query, GetMessageFactory<Q>(MaxMessageBodySize), message, false, channel: channel, messageHeader: messageHeader);
            var serviceConnection = await GetConnectionsAsync(serviceMessage.Channel, typeof(Q), serviceMessage.Header);
            return await ExecuteQueryAsync<Q, R>(serviceConnection.MessageServiceConnection, serviceMessage, timeout: timeout, responseChannel: responseChannel, cancellationToken: cancellationToken);
        }

        ValueTask<QueryResult<R>> IContractConnection.QueryAsync<Q, R>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
            => ProcessQueryAsync<Q, R>(message, timeout: timeout, channel: channel, responseChannel: responseChannel, messageHeader: messageHeader, cancellationToken: cancellationToken);

        async ValueTask<QueryResult<object>> IContractConnection.QueryAsync<Q>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader,
            CancellationToken cancellationToken)
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var responseType = (typeof(Q).GetCustomAttribute<QueryResponseTypeAttribute>(false)?.ResponseType)??throw new UnknownResponseTypeException("ResponseType", typeof(Q));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            var methodInfo = typeof(MappedConnection).GetMethod(nameof(MappedConnection.ProcessQueryAsync), BindingFlags.NonPublic | BindingFlags.Instance)!.MakeGenericMethod(typeof(Q), responseType!);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            try
            {
                return Utility.ConvertResultFromObject(await Utility.InvokeMethodAsync(
                    methodInfo,
                    this,
                    [
                        message,
                        timeout,
                        channel,
                        responseChannel,
                        messageHeader,
                        cancellationToken
                    ])
                )!;
            }
            catch (TimeoutException)
            {
                throw new QueryTimeoutException();
            }
        }

        protected override async ValueTask<ISubscription> ProduceSubscribeQueryResponseAsync<Q, R>(Func<IReceivedMessage<Q>, ValueTask<QueryResponseMessage<R>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, CancellationToken cancellationToken)
            where Q : class
            where R : class
        {
            var queryMessageFactory = GetMessageFactory<Q>(MaxMessageBodySize, ignoreMessageHeader);
            var responseMessageFactory = GetMessageFactory<R>(MaxMessageBodySize);
            (var serviceConnection, channel) = await GetConnectionsAsync<Q>(channel, ChannelMapper.MapTypes.QuerySubscription);
            return await CreateSubscriptionAsync<Q, R>(queryMessageFactory, responseMessageFactory, serviceConnection.MessageServiceConnection, messageReceived, errorReceived, channel, group, synchronous, cancellationToken);
        }
        #endregion
    }
}
