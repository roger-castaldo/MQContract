using MQContract.Interfaces.Service;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MQContract.RabbitMQ
{
    internal class Subscription : IServiceSubscription
    {
        private readonly IModel channel;
        private readonly Guid subscriptionID = Guid.NewGuid();
        private readonly string consumerTag;

        public Subscription(IConnection conn,string channel,string group, Action<BasicDeliverEventArgs,IModel,Func<ValueTask>> messageReceived, Action<Exception> errorReceived)
        {
            this.channel = conn.CreateModel();
            this.channel.QueueBind(group, channel, subscriptionID.ToString());
            this.channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(this.channel);
            consumer.Received+=(sender, @event) =>
            {
                messageReceived(
                    @event,
                    this.channel,
                    () =>
                    {
                        this.channel.BasicAck(@event.DeliveryTag, false);
                        return ValueTask.CompletedTask;
                    }
                );
            };

            consumerTag = this.channel.BasicConsume(group, false, consumer);
        }

        public ValueTask EndAsync()
        {
            channel.BasicCancel(consumerTag);
            channel.Close();
            return ValueTask.CompletedTask;
        }
    }
}
