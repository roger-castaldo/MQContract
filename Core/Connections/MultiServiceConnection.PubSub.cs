using MQContract.Interfaces;
using MQContract.Messages;
using MQContract.Subscriptions;

namespace MQContract.Connections
{
    internal partial class MultiServiceConnection
    {
        private static async ValueTask<ChildTransmissionResult> AwaitTransmission(string connectionName,Func<ValueTask<TransmissionResult>> transmit)
        {
            var result = await transmit();
            return new(connectionName, result.Error);
        }

        async ValueTask<MultiTransmissionResult> IMultiServiceContractConnection.PublishAsync<T>(T message, string? channel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            var serviceMessage = await ProduceServiceMessageAsync<T>(ChannelMapper.MapTypes.Publish, GetMessageFactory<T>(connectionList.MaxMessageBodySize), message, false, channel, messageHeader);
            var connections = await GetConnectionsAsync(serviceMessage.Channel, typeof(T), serviceMessage.Header);
            await publishLock.WaitAsync(cancellationToken);
            var results = await connections
                .WhenAll(c => AwaitTransmission(c.ServiceConnectionName, ()=>c.MessageServiceConnection.PublishAsync(serviceMessage, cancellationToken)));
            publishLock.Release();
            return new(serviceMessage.ID, results);
        }

        async ValueTask<IEnumerable<MultiTransmissionResult>> IMultiServiceContractConnection.BulkPublishAsync<T>(IEnumerable<(T message, MessageHeader? messageHeader)> messages, string? channel, CancellationToken cancellationToken)
        {
            var serviceMessages = await 
            messages.WhenAll(m =>
                    ProduceServiceMessageAsync<T>(ChannelMapper.MapTypes.Publish, GetMessageFactory<T>(connectionList.MaxMessageBodySize), m.message, false, channel, m.messageHeader)
            );
            var connections = await GetConnectionsAsync(serviceMessages.First().Channel, typeof(T), serviceMessages.First().Header);
            await publishLock.WaitAsync(cancellationToken);
            var transmissionResults = await Task.WhenAll(connections.Select(c => Task<MultiTransmissionResult>.Run(async () =>
            {
                var result = await BulkPublishAsync(serviceMessages, c.MessageServiceConnection, cancellationToken);
                return result.Select((res,index)=>new MultiTransmissionResult(serviceMessages.ElementAt(index).ID, [new(c.ServiceConnectionName,res.Error)]));
            })));
            publishLock.Release();
            return transmissionResults
                .SelectMany(mtr=>mtr)
                .GroupBy(mtr=>mtr.ID)
                .Select(grp=>new MultiTransmissionResult(grp.Key,grp.SelectMany(g=>g.Results)));
        }

        protected override async ValueTask<ISubscription> CreateSubscriptionAsync<T>(Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, CancellationToken cancellationToken)
            where T : class
        {
            var messageFactory = GetMessageFactory<T>(connectionList.MaxMessageBodySize,ignoreMessageHeader);
            channel = await Utility.GetChannelAsync<T>((originalChannel)=>MapChannel(ChannelMapper.MapTypes.PublishSubscription,originalChannel),channel);
            var connections = await GetConnectionsAsync(channel, typeof(T), new MessageHeader([]));
            return new SubscriptionCollection(await connections.WhenAll(conn=>
                CreateSubscriptionAsync<T>(
                    messageFactory,
                    conn.MessageServiceConnection,
                    messageReceived,
                    errorReceived,
                    channel,
                    group,
                    synchronous,
                    cancellationToken
                ))
            );
        }
    }
}
