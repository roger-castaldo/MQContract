using MQContract.Interfaces.Service;
using MQContract.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.RabbitMQ
{
    internal class Subscription : IServiceSubscription
    {
        private readonly IModel channel;
        private readonly Guid subscriptionID = Guid.NewGuid();
        private readonly string consumerTag;

        public Subscription(IConnection conn,string channel,string group, Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved)
        {
            this.channel = conn.CreateModel();
            this.channel.QueueBind(group, channel, subscriptionID.ToString());
            var consumer = new EventingBasicConsumer(this.channel);
            consumer.Received+=(sender, @event) =>
            {
                messageRecieved(
                    Connection.ConvertMessage(
                        @event, 
                        channel, 
                        () =>
                        {
                            this.channel.BasicAck(@event.DeliveryTag, false);
                            return ValueTask.CompletedTask;
                        }
                    )
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
