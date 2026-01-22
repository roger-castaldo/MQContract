using MQContract.Messages;
using NATS.Client.JetStream;

namespace MQContract.NATS.Subscriptions
{
    internal class StreamSubscription(INatsJSConsumer consumer, Func<ReceivedServiceMessage, ValueTask> messageReceived,
        Action<Exception> errorReceived)
        : SubscriptionBase()
    {
        protected override async Task RunAction()
        {
            while (!CancelToken.IsCancellationRequested)
            {
                try
                {
                    await consumer.RefreshAsync(CancelToken); // or try to recreate consumer

                    await foreach (var msg in consumer.ConsumeAsync<byte[]>().WithCancellation(CancelToken))
                    {
                        var ackSource = new TaskCompletionSource();
                        try
                        {
                            await Task.WhenAny(
                                messageReceived(ExtractMessage(msg, async () =>
                                {
                                    await msg.AckAsync(cancellationToken: CancelToken);
                                    ackSource.TrySetResult();
                                })).AsTask(),
                                ackSource.Task
                            );
                        }
                        catch (Exception ex)
                        {
                            errorReceived(ex);
                            await msg.NakAsync(cancellationToken: CancelToken);
                        }
                    }
                }
                catch (NatsJSProtocolException e)
                {
                    errorReceived(e);
                }
                catch (NatsJSException e)
                {
                    errorReceived(e);
                    // log exception
                    await Task.Delay(1000, CancelToken); // backoff
                }
            }
        }
    }
}
