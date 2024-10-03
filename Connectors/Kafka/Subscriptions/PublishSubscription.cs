using MQContract.Messages;

namespace MQContract.Kafka.Subscriptions
{
    internal class PublishSubscription(Confluent.Kafka.IConsumer<string, byte[]> consumer, Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel)
        : SubscriptionBase(consumer,channel)
    {
        protected override ValueTask RunAction()
        {
            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    var msg = Consumer.Consume(cancellationToken:cancelToken.Token);
                    var headers = Connection.ExtractHeaders(msg.Message.Headers, out var messageTypeID);
                    messageReceived(new ReceivedServiceMessage(
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
                    errorReceived(ex);
                }
                finally { }
            }
            return ValueTask.CompletedTask;
        }
    }
}
