using Confluent.Kafka;
using MQContract.NATS.Subscriptions;

namespace MQContract.Kafka.Subscriptions
{
    internal class QuerySubscription(Confluent.Kafka.IConsumer<string, byte[]> consumer, Func<Message<string, byte[]>, ValueTask> messageRecieved, Action<Exception> errorRecieved, string channel, CancellationToken cancellationToken)
        : SubscriptionBase(consumer,channel,cancellationToken)
    {
        protected override async ValueTask RunAction()
        {
            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    var msg = Consumer.Consume();
                    await messageRecieved(msg.Message);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    errorRecieved(ex);
                }
            }
        }
    }
}
