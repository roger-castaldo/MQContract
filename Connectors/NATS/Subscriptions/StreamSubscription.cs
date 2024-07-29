using MQContract.Interfaces.Service;
using MQContract.Messages;
using MQContract.NATS.Messages;
using MQContract.NATS.Serialization;
using NATS.Client.JetStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                    await foreach (var msg in consumer.ConsumeAsync<NatsMessage>(serializer: MessageSerializer<NatsMessage>.Default).WithCancellation(cancelToken.Token))
                    {
                        var success = true;
                        try
                        {
                            messageRecieved(new(
                                msg.Data?.ID??string.Empty,
                                msg.Data?.MessageTypeID??string.Empty,
                                msg.Subject,
                                new MQContract.NATS.Messages.MessageHeader(msg.Headers),
                                msg.Data?.Data??new ReadOnlyMemory<byte>()
                            ));
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
