using MQContract.Messages;
using NATS.Client.Core;

namespace MQContract.NATS.Subscriptions
{
    internal class PublishSubscription(IAsyncEnumerable<NatsMsg<byte[]>> asyncEnumerable,
        Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived) 
        : SubscriptionBase()
    {
        protected override async Task RunAction()
        {
            await foreach (var msg in asyncEnumerable.WithCancellation(CancelToken))
            {
                try
                {
                    messageReceived(ExtractMessage(msg));
                }
                catch (Exception ex)
                {
                    errorReceived(ex);
                }
            }
        }
    }
}
