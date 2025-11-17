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

        protected override ConnectionHealthCheck? ProduceConnectionHealthCheck()
            => new(connection: serviceConnection);

        protected override async ValueTask CloseAsync()
            => await (serviceConnection?.CloseAsync()??ValueTask.CompletedTask);

        protected override async ValueTask InternalDisposeAsync()
        {
            if (serviceConnection is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(true);
            else if (serviceConnection is IDisposable disposable)
                disposable.Dispose();
            publishLock.Dispose();
        }

        #region PubSub
        protected override ValueTask<ISubscription> CreateSubscriptionAsync<T>(Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<T>? messageFilters, bool synchronous, CancellationToken cancellationToken)
            => CreateSubscriptionAsync<T>(
                GetMessageFactory<T>(ignoreMessageHeader),
                serviceConnection,
                messageReceived,
                errorReceived,
                channel,
                group,
                synchronous,
                null,
                messageFilters,
                cancellationToken
            );

        async ValueTask<TransmissionResult> IContractConnection.PublishAsync<T>(T message, string? channel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Publishing message {T} on {Channel}", typeof(T), channel);
            using var activity = StartActivity(Constants.PublishActivityName, serviceConnection: serviceConnection);
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
            using var activity = StartActivity(Constants.BulkPublishActivityName, serviceConnection:serviceConnection);
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
                        messageHeader: m.messageHeader
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
        async ValueTask<QueryResult<TQueryResponse>> IContractConnection.QueryAsync<TQuery, TQueryResponse>(TQuery message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Executing QueryResponse of {TQuery}, expecting {TQueryResponse} on {Channel} with {ResponseChannel}", typeof(TQuery), typeof(TQueryResponse), channel, responseChannel);
            using var activity = StartActivity(Constants.PublishQueryActivityName, serviceConnection: serviceConnection);
            var serviceMessage = await ProduceServiceMessageAsync<TQuery>(
                ChannelMapper.MapTypes.Query, 
                GetMessageFactory<TQuery>(), 
                message, 
                false, 
                activity, 
                maxMessageSize: serviceConnection.MaxMessageBodySize,
                channel: channel, 
                messageHeader: messageHeader
            );
            return await ExecuteQueryAsync<TQuery, TQueryResponse>(serviceConnection, serviceMessage, activity, timeout: timeout, responseChannel: responseChannel, cancellationToken: cancellationToken);
        }

        private static readonly MethodInfo fullGenericQueryAsync = typeof(IContractConnection).GetMethods().First(m => Equals(m.Name, nameof(IContractConnection.QueryAsync))
            && m.GetGenericArguments().Length==2);

        async ValueTask<QueryResult<object>> IContractConnection.QueryAsync<TQuery>(TQuery message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader,
            CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Attempting to get response type for QueryResponse for {TQuery} on {Channel} with {ResponseChannel}", typeof(TQuery), channel, responseChannel);
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var responseType = (typeof(TQuery).GetCustomAttribute<QueryMessageAttribute>(false)?.ResponseType)??throw new UnknownResponseTypeException("ResponseType", typeof(TQuery));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            Logger?.LogInformation("Obtained {ResponseType} for QueryResponse for {TQuery} on {Channel} with {ResponseChannel}", responseType, typeof(TQuery), channel, responseChannel);
            try
            {
                return Utility.ConvertResultFromObject(await Utility.InvokeMethodAsync(
                    fullGenericQueryAsync.MakeGenericMethod(typeof(TQuery), responseType!),
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

        protected override async ValueTask<ISubscription> ProduceSubscribeQueryResponseAsync<TQuery, TQueryResponse>(Func<IReceivedMessage<TQuery>, ValueTask<QueryResponseMessage<TQueryResponse>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, MessageFilters<TQuery>? messageFilter, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebug("Producing QueryResponse Subscription for {TQuery} responding with {TQueryResponse} on {Channel} in {Group}", typeof(TQuery), typeof(TQueryResponse), channel, group);
            var queryMessageFactory = GetMessageFactory<TQuery>(ignoreMessageHeader);
            var responseMessageFactory = GetMessageFactory<TQueryResponse>();
            return await CreateSubscriptionAsync<TQuery, TQueryResponse>(
                queryMessageFactory, 
                responseMessageFactory, 
                serviceConnection, 
                messageReceived, 
                errorReceived, 
                channel, 
                group, 
                synchronous, 
                null, 
                messageFilter,
                cancellationToken
            );
        }
        #endregion
    }
}
