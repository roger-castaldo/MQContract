﻿using MQContract.Messages;
using StackExchange.Redis;

namespace MQContract.Redis.Subscriptions
{
    internal class PubSubscription(Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, IDatabase database, Guid connectionID, string channel, string? group)
        : SubscriptionBase(errorRecieved,database,connectionID,channel,group)
    {
        protected override ValueTask ProcessMessage(StreamEntry streamEntry, string channel, string? group)
        {
            (var message,_,_) = Connection.ConvertMessage(
                    streamEntry.Values,
                    channel,
                    () => Acknowledge(streamEntry.Id)
                 );
            messageRecieved(message);
            return ValueTask.CompletedTask;
        }
    }
}
