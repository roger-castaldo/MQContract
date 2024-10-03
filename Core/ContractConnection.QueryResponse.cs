using MQContract.Attributes;
using MQContract.Interfaces.Service;
using MQContract.Interfaces;
using MQContract.Messages;
using MQContract.Subscriptions;
using System.Reflection;

namespace MQContract
{
    public partial class ContractConnection
    {
        private async ValueTask<QueryResult<R>> ProcessPubSubQuery<Q, R>(string? responseChannel, TimeSpan? realTimeout, ServiceMessage serviceMessage, CancellationToken cancellationToken)
            where Q : class
            where R : class
        {
            responseChannel ??=typeof(Q).GetCustomAttribute<QueryResponseChannelAttribute>()?.Name;
            ArgumentNullException.ThrowIfNullOrWhiteSpace(responseChannel);
            var replyChannel = await MapChannel(ChannelMapper.MapTypes.QueryResponse, responseChannel!);
            var callID = Guid.NewGuid();
            var (tcs, token) = await QueryResponseHelper.StartResponseListenerAsync(
                serviceConnection,
                realTimeout??TimeSpan.FromMinutes(1),
                indentifier,
                callID,
                replyChannel,
                cancellationToken
            );
            var msg = QueryResponseHelper.EncodeMessage(
                serviceMessage,
                indentifier,
                callID,
                replyChannel,
                null
            );
            await serviceConnection.PublishAsync(msg, cancellationToken: cancellationToken);
            try
            {
                await tcs.Task.WaitAsync(cancellationToken);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                    await token.CancelAsync();
            }
            return await ProduceResultAsync<R>(tcs.Task.Result);
        }

        private async ValueTask<ServiceQueryResult> ProcessInboxMessage(IInboxQueryableMessageServiceConnection inboxMessageServiceConnection, ServiceMessage serviceMessage, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var messageID = Guid.NewGuid();
            await inboxSemaphore.WaitAsync(cancellationToken);
            inboxSubscription ??= await inboxMessageServiceConnection.EstablishInboxSubscriptionAsync(
                    async (message) =>
                    {
                        await inboxSemaphore.WaitAsync();
                        if (message.Acknowledge!=null)
                            await message.Acknowledge();
                        if (inboxResponses.TryGetValue(message.CorrelationID,out var taskCompletionSource))
                        {
                            taskCompletionSource.TrySetResult(new(
                                message.ID,
                                message.Header,
                                message.MessageTypeID,
                                message.Data
                            ));
                        }
                        inboxSemaphore.Release();
                    },
                    cancellationToken
                );
            var tcs = new TaskCompletionSource<ServiceQueryResult>();
            inboxResponses.Add(messageID, tcs);
            inboxSemaphore.Release();
            using var token = new CancellationTokenSource();
            var reg = cancellationToken.Register(() => token.Cancel());
            token.Token.Register(async () => {
                await reg.DisposeAsync();
                if (!tcs.Task.IsCompleted)
                    tcs.TrySetException(new QueryTimeoutException());
            });
            token.CancelAfter(timeout);
            var result = await inboxMessageServiceConnection.QueryAsync(serviceMessage, messageID, cancellationToken);
            if (result.IsError)
            {
                await inboxSemaphore.WaitAsync();
                inboxResponses.Remove(messageID);
                inboxSemaphore.Release();
                throw new QuerySubmissionFailedException(result.Error!);
            }
            try
            {
                await tcs.Task.WaitAsync(cancellationToken);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                    await token.CancelAsync();
                await inboxSemaphore.WaitAsync();
                inboxResponses.Remove(messageID);
                inboxSemaphore.Release();
            }
            return tcs.Task.Result;
        }

        private async ValueTask<QueryResult<R>> ExecuteQueryAsync<Q, R>(Q message, TimeSpan? timeout = null, string? channel = null, string? responseChannel = null, MessageHeader? messageHeader = null, CancellationToken cancellationToken = new CancellationToken())
                where Q : class
                where R : class
        {
            var realTimeout = timeout??typeof(Q).GetCustomAttribute<MessageResponseTimeoutAttribute>()?.TimeSpanValue;
            var serviceMessage = await ProduceServiceMessageAsync<Q>(ChannelMapper.MapTypes.Query, GetMessageFactory<Q>(), message, false,channel: channel, messageHeader: messageHeader);
            if (serviceConnection is IQueryResponseMessageServiceConnection queryableMessageServiceConnection)
                return await ProduceResultAsync<R>(
                    await queryableMessageServiceConnection.QueryAsync(
                        serviceMessage,
                        realTimeout??queryableMessageServiceConnection.DefaultTimeout,
                        cancellationToken
                    )
                );
            else if (serviceConnection is IInboxQueryableMessageServiceConnection inboxMessageServiceConnection)
                return await ProduceResultAsync<R>(
                    await ProcessInboxMessage(inboxMessageServiceConnection, serviceMessage, realTimeout??inboxMessageServiceConnection.DefaultTimeout,cancellationToken)
                );
            return await ProcessPubSubQuery<Q, R>(responseChannel, realTimeout, serviceMessage, cancellationToken);
        }

        private async ValueTask<QueryResult<R>> ProduceResultAsync<R>(ServiceQueryResult queryResult) where R : class
        {
            QueryResult<R> result;
            try
            {
                (var resultMessage, var messageHeader) = await DecodeServiceMessageAsync<R>(ChannelMapper.MapTypes.QueryResponse, GetMessageFactory<R>(true), new(queryResult.ID, queryResult.MessageTypeID, string.Empty, queryResult.Header, queryResult.Data));
                result = new QueryResult<R>(
                    queryResult.ID,
                    messageHeader,
                    Result: resultMessage
                );
            }
            catch (QueryResponseException qre)
            {
                return new(
                    queryResult.ID,
                    queryResult.Header,
                    Result: default,
                    Error: qre.Message
                );
            }
            catch (Exception ex)
            {
                return new(
                    queryResult.ID,
                    queryResult.Header,
                    Result: default,
                    Error: ex.Message
                );
            }
            return result;
        }
        private async ValueTask<ISubscription> ProduceSubscribeQueryResponseAsync<Q, R>(Func<IReceivedMessage<Q>, ValueTask<QueryResponseMessage<R>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, CancellationToken cancellationToken)
            where Q : class
            where R : class
        {
            var queryMessageFactory = GetMessageFactory<Q>(ignoreMessageHeader);
            var responseMessageFactory = GetMessageFactory<R>();
            var subscription = new QueryResponseSubscription<Q>(
                async (message, replyChannel) =>
                {
                    (var taskMessage, var messageHeader) = await DecodeServiceMessageAsync<Q>(
                        ChannelMapper.MapTypes.QuerySubscription,
                        queryMessageFactory,
                        message
                    );
                    var result = await messageReceived(new ReceivedMessage<Q>(message.ID, taskMessage!, messageHeader, message.ReceivedTimestamp, DateTime.Now));
                    return await ProduceServiceMessageAsync<R>(
                        ChannelMapper.MapTypes.QueryResponse,
                        responseMessageFactory,
                        result.Message,
                        true,
                        replyChannel,
                        new(result.Headers)
                    );
                },
                errorReceived,
                (originalChannel) => MapChannel(ChannelMapper.MapTypes.QuerySubscription, originalChannel),
                channel: channel,
                group: group,
                synchronous: synchronous,
                logger: logger);
            if (await subscription.EstablishSubscriptionAsync(serviceConnection, cancellationToken))
                return subscription;
            throw new SubscriptionFailedException();
        }

        async ValueTask<QueryResult<R>> IContractConnection.QueryAsync<Q, R>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
            => await ExecuteQueryAsync<Q, R>(message, timeout: timeout, channel: channel, responseChannel: responseChannel, messageHeader: messageHeader, cancellationToken: cancellationToken);

        async ValueTask<QueryResult<object>> IContractConnection.QueryAsync<Q>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader,
            CancellationToken cancellationToken)
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var responseType = (typeof(Q).GetCustomAttribute<QueryResponseTypeAttribute>(false)?.ResponseType)??throw new UnknownResponseTypeException("ResponseType", typeof(Q));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            var methodInfo = typeof(ContractConnection).GetMethod(nameof(ContractConnection.ExecuteQueryAsync), BindingFlags.NonPublic | BindingFlags.Instance)!.MakeGenericMethod(typeof(Q), responseType!);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            dynamic? queryResult;
            try
            {
                queryResult = (dynamic?)await Utility.InvokeMethodAsync(
                    methodInfo,
                    this, [
                        message,
                    timeout,
                    channel,
                    responseChannel,
                    messageHeader,
                    cancellationToken
                    ]
                );
            }
            catch (TimeoutException)
            {
                throw new QueryTimeoutException();
            }
            return new QueryResult<object>(queryResult?.ID??string.Empty, queryResult?.Header??new MessageHeader([]), queryResult?.Result, queryResult?.Error);
        }

        ValueTask<ISubscription> IContractConnection.SubscribeQueryAsyncResponseAsync<Q, R>(Func<IReceivedMessage<Q>, ValueTask<QueryResponseMessage<R>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => ProduceSubscribeQueryResponseAsync<Q, R>(messageReceived, errorReceived, channel, group, ignoreMessageHeader, false, cancellationToken);

        ValueTask<ISubscription> IContractConnection.SubscribeQueryResponseAsync<Q, R>(Func<IReceivedMessage<Q>, QueryResponseMessage<R>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => ProduceSubscribeQueryResponseAsync<Q, R>((msg) =>
        {
            var result = messageReceived(msg);
            return ValueTask.FromResult(result);
        }, errorReceived, channel, group, ignoreMessageHeader, true, cancellationToken);
    }
}
