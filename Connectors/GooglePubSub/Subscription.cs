using Google.Cloud.PubSub.V1;
using MQContract.Interfaces.Service;
using MQContract.Messages;


namespace MQContract.GooglePubSub
{
    internal class Subscription(SubscriberServiceApiClient subscriberClientApi, SubscriptionName subscriptionName, Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel) : IServiceSubscription, IAsyncDisposable
    {
        protected readonly CancellationTokenSource cancelToken = new();
        private bool disposedValue;

        public void Start()
        {
            _ = Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var msg = await subscriberClientApi.PullAsync(subscriptionName, 1, cancelToken.Token);
                        if (msg!=null)
                            await messageReceived(Connection.ConvertMessage(
                                msg.ReceivedMessages[0],
                                channel,
                                async () => await subscriberClientApi.AcknowledgeAsync(subscriptionName, [msg.ReceivedMessages[0].AckId], cancelToken.Token)
                            )).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        errorReceived(ex);
                    }
                }
            });
        }

        public async ValueTask EndAsync()
        {
            if (!cancelToken.IsCancellationRequested)
                await cancelToken.CancelAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                await EndAsync();
                cancelToken.Dispose();
            }
        }
    }
}
