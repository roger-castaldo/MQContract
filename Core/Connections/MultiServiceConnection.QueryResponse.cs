using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Messages;
using MQContract.Subscriptions;
using System.Reflection;

namespace MQContract.Connections
{
    internal partial class MultiServiceConnection
    {
        private async ValueTask<IEnumerable<QueryResult<R>>> ProcessQueryAsync<Q, R>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
            where Q : class
            where R : class
        {
            var serviceMessage = await ProduceServiceMessageAsync<Q>(ChannelMapper.MapTypes.Query, GetMessageFactory<Q>(connectionList.MaxMessageBodySize), message, false, channel: channel, messageHeader: messageHeader);
            var connections = await GetConnectionsAsync(serviceMessage.Channel, typeof(Q), serviceMessage.Header);
            return await connections
                .WhenAll(conn => ExecuteQueryAsync<Q, R>(conn.MessageServiceConnection, serviceMessage, timeout: timeout, responseChannel: responseChannel, cancellationToken: cancellationToken));
        }
        ValueTask<IEnumerable<QueryResult<R>>> IMultiServiceContractConnection.QueryAsync<Q, R>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
            => ProcessQueryAsync<Q, R>(message, timeout: timeout, channel: channel, responseChannel: responseChannel, messageHeader: messageHeader, cancellationToken: cancellationToken);

        async ValueTask<IEnumerable<QueryResult<object>>> IMultiServiceContractConnection.QueryAsync<Q>(Q message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var responseType = (typeof(Q).GetCustomAttribute<QueryResponseTypeAttribute>(false)?.ResponseType)??throw new UnknownResponseTypeException("ResponseType", typeof(Q));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            var methodInfo = typeof(MultiServiceConnection).GetMethod(nameof(MultiServiceConnection.ProcessQueryAsync), BindingFlags.NonPublic | BindingFlags.Instance)!.MakeGenericMethod(typeof(Q), responseType!);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            IEnumerable<object> results;
            try
            {
                results = (IEnumerable<object>)(await Utility.InvokeMethodAsync(
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
            return results.Select(o => Utility.ConvertResultFromObject(o)!);
        }

        protected override async ValueTask<ISubscription> ProduceSubscribeQueryResponseAsync<Q, R>(Func<IReceivedMessage<Q>, ValueTask<QueryResponseMessage<R>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, CancellationToken cancellationToken)
        {
            var queryMessageFactory = GetMessageFactory<Q>(connectionList.MaxMessageBodySize, ignoreMessageHeader);
            var responseMessageFactory = GetMessageFactory<R>(connectionList.MaxMessageBodySize);
            channel = await Utility.GetChannelAsync<Q>((originalChannel) => MapChannel(ChannelMapper.MapTypes.PublishSubscription, originalChannel), channel);
            var connections = await GetConnectionsAsync(channel, typeof(Q), new MessageHeader([]));
            return new SubscriptionCollection(await connections
                .WhenAll(conn =>
                    CreateSubscriptionAsync<Q, R>(
                       queryMessageFactory, 
                       responseMessageFactory, 
                       conn.MessageServiceConnection, 
                       messageReceived, 
                       errorReceived, 
                       channel, 
                       group, 
                       synchronous, 
                       cancellationToken
                    )
               )
            );
        }
    }
}
