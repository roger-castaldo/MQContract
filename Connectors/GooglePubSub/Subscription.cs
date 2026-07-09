using Google.Cloud.PubSub.V1;
using MQContract.Messages;


namespace MQContract.GooglePubSub;

internal class Subscription(SubscriberServiceApiClient subscriberClientApi, SubscriptionName subscriptionName, Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel) 
    : BaseLoopSubscription(errorReceived)
{
    protected override async ValueTask RecieveMessageAsync(CancellationToken cancelToken)
    {
        try
        {
            var msg = await subscriberClientApi.PullAsync(subscriptionName, 1, cancelToken);
            if (msg!=null)
            {
                var ackSource = new TaskCompletionSource();
                await Task.WhenAny(
                    messageReceived(Connection.ConvertMessage(
                        msg.ReceivedMessages[0],
                        channel,
                        async () =>
                        {
                            await subscriberClientApi.AcknowledgeAsync(subscriptionName, [msg.ReceivedMessages[0].AckId], cancelToken);
                            ackSource.SetResult();
                        }
                    )).AsTask(),
                    ackSource.Task
                );
            }
        }catch(Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Cancelled)
        {
            // Ignore cancellation exceptions
        }
    }
}
