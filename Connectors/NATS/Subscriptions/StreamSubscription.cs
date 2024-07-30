using MQContract.Messages;
using NATS.Client.JetStream;

namespace MQContract.NATS.Subscriptions
{
    internal class StreamSubscription(INatsJSConsumer consumer, Action<RecievedServiceMessage> messageRecieved, 
        Action<Exception> errorRecieved, CancellationToken cancellationToken) 
        : SubscriptionBase(cancellationToken)
    {
        protected override async Task RunAction()
        {
            while (!cancelToken.Token.IsCancellationRequested)
            {
                try
                {
                    await consumer.RefreshAsync(cancelToken.Token); // or try to recreate consumer

                    await foreach (var msg in consumer.ConsumeAsync<byte[]>().WithCancellation(cancelToken.Token))
                    {
                        var success = true;
                        try
                        {
                            messageRecieved(ExtractMessage(msg));
                        }
                        catch (Exception ex)
                        {
                            success=false;
                            errorRecieved(ex);
                            await msg.NakAsync(cancellationToken: cancelToken.Token);
                        }
                        if (success)
                            await msg.AckAsync(cancellationToken: cancelToken.Token);
                    }
                }
                catch (NatsJSProtocolException e)
                {
                    errorRecieved(e);
                }
                catch (NatsJSException e)
                {
                    errorRecieved(e);
                    // log exception
                    await Task.Delay(1000, cancelToken.Token); // backoff
                }
            }
        }
    }
}
