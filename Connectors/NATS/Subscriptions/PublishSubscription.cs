using MQContract.Messages;
using NATS.Client.Core;

namespace MQContract.NATS.Subscriptions
{
    internal class PublishSubscription(IAsyncEnumerable<NatsMsg<byte[]>> asyncEnumerable,
        Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved) 
        : SubscriptionBase()
    {
        protected override async Task RunAction()
        {
            await foreach (var msg in asyncEnumerable.WithCancellation(CancelToken))
            {
                try
                {
                    messageRecieved(ExtractMessage(msg));
                }
                catch (Exception ex)
                {
                    errorRecieved(ex);
                }
            }
        }
    }
}
