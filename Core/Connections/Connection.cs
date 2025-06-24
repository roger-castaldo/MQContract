using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Diagnostics;
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
            => await (serviceConnection?.CloseAsync()??ValueTask.CompletedTask);

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
            => CreateSubscriptionAsync<T>(
                GetMessageFactory<T>(ignoreMessageHeader),
                serviceConnection,
                messageReceived,
                errorReceived,
                channel,
                group,
                synchronous,
                null,
                cancellationToken
            );

        async ValueTask<TransmissionResult> IContractConnection.PublishAsync<T>(T message, string? channel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Publishing message {T} on {Channel}", typeof(T), channel);
            (var activity, messageHeader) = StartActivity(Constants.PublishActivityName, ActivityKind.Producer, messageHeader, serviceConnection);
            var serviceMessage = await ProduceServiceMessageAsync<T>(
                ChannelMapper.MapTypes.Publish, 
                GetMessageFactory<T>(), 
                message, 
                false, 
                activity, 
                maxMessageSize: serviceConnection.MaxMessageBodySize,
                channel: channel, 
                messageHeader: messageHeader
            );
            return await PublishMessageAsync<T>(publishLock, serviceMessage, serviceConnection, activity, null, cancellationToken);
        }

        async ValueTask<IEnumerable<TransmissionResult>> IContractConnection.BulkPublishAsync<T>(IEnumerable<(T message, MessageHeader? messageHeader)> messages, string? channel, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Bulk Publishing messages {T} on {Channel}", typeof(T), channel);
            (var activity, var headers) = StartActivity(Constants.BulkPublishActivityName, ActivityKind.Producer, null, serviceConnection);
            activity?.SetTag(Constants.BulkPublishCountTag, messages.Count());
            var serviceMessages = await
                messages.WhenAll(m =>
                    ProduceServiceMessageAsync<T>(
                        ChannelMapper.MapTypes.Publish, 
                        GetMessageFactory<T>(), 
                        m.message, 
                        false, 
                        activity, 
                        maxMessageSize:serviceConnection.MaxMessageBodySize,
                        channel:channel, 
                        messageHeader: new(m.messageHeader, headers)
                    )
                );
            await publishLock.WaitAsync(cancellationToken);
            var result = await BulkPublishAsync<T>(serviceMessages, serviceConnection, activity, cancellationToken);
            publishLock.Release();
            activity?.SetStatus(result.Any(r => r.IsError) ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            activity?.Stop();
            return result;
        }
        #endregion

        #region QueryResponse
        async ValueTask<QueryResult<R>> IContractConnection.QueryAsync<Q, R>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Executing QueryResponse of {Q}, expecting {R} on {Channel} with {ResponseChannel}", typeof(Q), typeof(R), channel, responseChannel);
            (var activity, messageHeader) = StartActivity(Constants.PublishQueryActivityName, ActivityKind.Producer, messageHeader, serviceConnection);
            var serviceMessage = await ProduceServiceMessageAsync<Q>(
                ChannelMapper.MapTypes.Query, 
                GetMessageFactory<Q>(), 
                message, 
                false, 
                activity, 
                maxMessageSize: serviceConnection.MaxMessageBodySize,
                channel: channel, 
                messageHeader: messageHeader
            );
            return await ExecuteQueryAsync<Q, R>(serviceConnection, serviceMessage, activity, timeout: timeout, responseChannel: responseChannel, cancellationToken: cancellationToken);
        }

        private static readonly MethodInfo fullGenericQueryAsync = typeof(IContractConnection).GetMethods().First(m => Equals(m.Name, nameof(IContractConnection.QueryAsync))
            && m.GetGenericArguments().Length==2);

        async ValueTask<QueryResult<object>> IContractConnection.QueryAsync<Q>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader,
            CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Attempting to get response type for QueryResponse for {Q} on {Channel} with {ResponseChannel}", typeof(Q), channel, responseChannel);
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var responseType = (typeof(Q).GetCustomAttribute<QueryResponseTypeAttribute>(false)?.ResponseType)??throw new UnknownResponseTypeException("ResponseType", typeof(Q));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            Logger?.LogInformation("Obtained {ResponseType} for QueryResponse for {Q} on {Channel} with {ResponseChannel}", responseType, typeof(Q), channel, responseChannel);
            try
            {
                return Utility.ConvertResultFromObject(await Utility.InvokeMethodAsync(
                    fullGenericQueryAsync.MakeGenericMethod(typeof(Q), responseType!),
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
        {
            using var scope = SetScope();
            Logger?.LogDebug("Producing QueryResponse Subscription for {Q} responding with {R} on {Channel} in {Group}", typeof(Q), typeof(R), channel, group);
            var queryMessageFactory = GetMessageFactory<Q>(ignoreMessageHeader);
            var responseMessageFactory = GetMessageFactory<R>();
            return await CreateSubscriptionAsync<Q, R>(
                queryMessageFactory, 
                responseMessageFactory, 
                serviceConnection, 
                messageReceived, 
                errorReceived, 
                channel, 
                group, 
                synchronous, 
                null, 
                cancellationToken
            );
        }
        #endregion
    }
}
