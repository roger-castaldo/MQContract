using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Messages;
using System.Reflection;

namespace MQContract.Connections
{
    internal partial class Connection
    {
        private async ValueTask<QueryResult<R>> ProcessQueryAsync<Q, R>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
            where Q : class
            where R : class
        {
            var serviceMessage = await ProduceServiceMessageAsync<Q>(ChannelMapper.MapTypes.Query, GetMessageFactory<Q>(serviceConnection.MaxMessageBodySize), message, false, channel: channel, messageHeader: messageHeader);
            return await ExecuteQueryAsync<Q, R>(serviceConnection, serviceMessage, timeout:timeout, responseChannel:responseChannel, cancellationToken: cancellationToken);
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

    }
}
