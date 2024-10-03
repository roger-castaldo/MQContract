using MQContract.Messages;
using StackExchange.Redis;

namespace MQContract.Redis.Subscriptions
{
    internal class QueryResponseSubscription(Func<ReceivedServiceMessage, ValueTask<ServiceMessage>> messageReceived, Action<Exception> errorReceived, IDatabase database, Guid connectionID, string channel, string? group)
        : SubscriptionBase(errorReceived,database,connectionID,channel,group)
    {
        protected override async ValueTask ProcessMessage(StreamEntry streamEntry, string channel, string? group)
        {
            (var message,var responseChannel,var timeout) = Connection.ConvertMessage(
                    streamEntry.Values,
                    channel,
                    ()=> Acknowledge(streamEntry.Id)
                 );
            var result = await messageReceived(message);
            await Database.StreamDeleteAsync(Channel, [streamEntry.Id]);
            await Database.StringSetAsync(responseChannel, Connection.EncodeMessage(result), expiry: timeout);
        }
    }
}
