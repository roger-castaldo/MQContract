using MQContract.Messages;
using MQContract.NATS.Subscriptions;

namespace MQContract.Kafka.Subscriptions
{
    internal class PublishSubscription(Confluent.Kafka.IConsumer<string, byte[]> consumer, Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, CancellationToken cancellationToken)
        : SubscriptionBase(consumer,channel,cancellationToken)
    {
        protected override Task RunAction()
        {
            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    var msg = Consumer.Consume(cancellationToken:cancelToken.Token);
                    var headers = Connection.ExtractHeaders(msg.Message.Headers, out var messageTypeID);
                    messageRecieved(new RecievedServiceMessage(
                        msg.Message.Key??string.Empty,
                        messageTypeID??string.Empty,
                        Channel,
                        headers,
                        msg.Message.Value
                    ));
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    errorRecieved(ex);
                }
                finally { }
            }
            return Task.CompletedTask;
        }
    }
}
