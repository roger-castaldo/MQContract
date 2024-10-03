using MQContract.Messages;
using NATS.Client.JetStream;

namespace MQContract.NATS.Subscriptions
{
    internal class StreamSubscription(INatsJSConsumer consumer, Action<ReceivedServiceMessage> messageReceived, 
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
                        var success = true;
                        try
                        {
                            messageReceived(ExtractMessage(msg));
                        }
                        catch (Exception ex)
                        {
                            success=false;
                            errorReceived(ex);
                            await msg.NakAsync(cancellationToken: CancelToken);
                        }
                        if (success)
                            await msg.AckAsync(cancellationToken: CancelToken);
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
