using MQContract.Messages;
using NATS.Client.Core;

namespace MQContract.NATS.Subscriptions;

internal class PublishSubscription(IAsyncEnumerable<NatsMsg<byte[]>> asyncEnumerable,
    Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived)
    : SubscriptionBase()
{
    protected override async Task RunAction()
    {
        await foreach (var msg in asyncEnumerable.WithCancellation(CancelToken))
        {
            try
            {
                await messageReceived(ExtractMessage(msg)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                errorReceived(ex);
            }
        }
    }
}
