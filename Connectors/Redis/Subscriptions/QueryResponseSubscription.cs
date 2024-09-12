using MQContract.Messages;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Redis.Subscriptions
{
    internal class QueryResponseSubscription(Func<RecievedServiceMessage, ValueTask<ServiceMessage>> messageRecieved, Action<Exception> errorRecieved, IDatabase database, Guid connectionID, string channel, string? group)
        : SubscriptionBase(errorRecieved,database,connectionID,channel,group)
    {
        protected override async ValueTask ProcessMessage(StreamEntry streamEntry, string channel, string? group)
        {
            (var message,var responseChannel,var timeout) = Connection.ConvertMessage(
                    streamEntry.Values,
                    channel,
                    ()=> Acknowledge(streamEntry.Id)
                 );
            var result = await messageRecieved(message);
            await Database.StreamDeleteAsync(Channel, [streamEntry.Id]);
            await Database.StringSetAsync(responseChannel, Connection.EncodeMessage(result), expiry: timeout);
        }
    }
}
