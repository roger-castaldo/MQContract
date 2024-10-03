using MQContract.Messages;
using StackExchange.Redis;

namespace MQContract.Redis.Subscriptions
{
    internal class PubSubscription(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, IDatabase database, Guid connectionID, string channel, string? group)
        : SubscriptionBase(errorReceived,database,connectionID,channel,group)
    {
        protected override ValueTask ProcessMessage(StreamEntry streamEntry, string channel, string? group)
        {
            (var message,_,_) = Connection.ConvertMessage(
                    streamEntry.Values,
                    channel,
                    () => Acknowledge(streamEntry.Id)
                 );
            messageReceived(message);
            return ValueTask.CompletedTask;
        }
    }
}
