using MQContract.Messages;
using MQContract.NATS.Messages;
using MQContract.NATS.Serialization;
using NATS.Client.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.NATS.Subscriptions
{
    internal class PublishSubscription(IAsyncEnumerable<NatsMsg<NatsMessage>> asyncEnumerable,
        Action<IRecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved,
        CancellationToken cancellationToken) : SubscriptionBase(cancellationToken)
    {
        protected override async Task RunAction()
        {
            await foreach (var msg in asyncEnumerable.WithCancellation(cancelToken.Token))
            {
                try
                {
                    messageRecieved(new RecievedMessage(new MessageHeader(msg.Headers), msg.Subject, msg.Data));
                }
                catch (Exception ex)
                {
                    errorRecieved(ex);
                }
            }
        }
    }
}
