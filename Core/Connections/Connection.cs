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
    internal class Connection(IMessageServiceConnection serviceConnection,
        IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null) : 
        AConnection<IContractedConnection>(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, logger, channelMapper),
        IContractedConnection
    {
        private readonly SemaphoreSlim publishLock = new(1, 1);

        ValueTask<PingResult> IContractConnection.PingAsync()
            => (serviceConnection is IPingableMessageServiceConnection pingableService ? pingableService.PingAsync() : throw new PingNotSupportedException());

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

        #region PubSub
        protected override ValueTask<ISubscription> CreateSubscriptionAsync<T>(Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, CancellationToken cancellationToken)
            where T : class
            => CreateSubscriptionAsync<T>(
                GetMessageFactory<T>(serviceConnection.MaxMessageBodySize, ignoreMessageHeader),
                serviceConnection,
                messageReceived,
                errorReceived,
                channel,
                group,
                synchronous,
                cancellationToken
            );

        async ValueTask<TransmissionResult> IContractConnection.PublishAsync<T>(T message, string? channel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            await publishLock.WaitAsync(cancellationToken);
            var result = await serviceConnection.PublishAsync(
                await ProduceServiceMessageAsync<T>(ChannelMapper.MapTypes.Publish, GetMessageFactory<T>(serviceConnection.MaxMessageBodySize), message, false, channel, messageHeader),
                cancellationToken
            );
            publishLock.Release();
            return result;
        }

        async ValueTask<IEnumerable<TransmissionResult>> IContractConnection.BulkPublishAsync<T>(IEnumerable<(T message, MessageHeader? messageHeader)> messages, string? channel, CancellationToken cancellationToken)
        {
            var serviceMessages = await
                messages.WhenAll(m =>
                    ProduceServiceMessageAsync<T>(ChannelMapper.MapTypes.Publish, GetMessageFactory<T>(serviceConnection.MaxMessageBodySize), m.message, false, channel, m.messageHeader)
                );
            await publishLock.WaitAsync(cancellationToken);
            var result = await BulkPublishAsync(serviceMessages, serviceConnection, cancellationToken);
            publishLock.Release();
            return result;
        }
        #endregion

        #region QueryResponse
        private async ValueTask<QueryResult<R>> ProcessQueryAsync<Q, R>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
            where Q : class
            where R : class
        {
            var serviceMessage = await ProduceServiceMessageAsync<Q>(ChannelMapper.MapTypes.Query, GetMessageFactory<Q>(serviceConnection.MaxMessageBodySize), message, false, channel: channel, messageHeader: messageHeader);
            return await ExecuteQueryAsync<Q, R>(serviceConnection, serviceMessage, timeout: timeout, responseChannel: responseChannel, cancellationToken: cancellationToken);
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
            var methodInfo = typeof(Connection).GetMethod(nameof(Connection.ProcessQueryAsync), BindingFlags.NonPublic | BindingFlags.Instance)!.MakeGenericMethod(typeof(Q), responseType!);
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
            var queryMessageFactory = GetMessageFactory<Q>(serviceConnection.MaxMessageBodySize, ignoreMessageHeader);
            var responseMessageFactory = GetMessageFactory<R>(serviceConnection.MaxMessageBodySize);
            return await CreateSubscriptionAsync<Q, R>(queryMessageFactory, responseMessageFactory, serviceConnection, messageReceived, errorReceived, channel, group, synchronous, cancellationToken);
        }
        #endregion
    }
}
