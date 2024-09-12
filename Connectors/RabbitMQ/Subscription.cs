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

        public Subscription(IConnection conn,string channel,string? group, Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved)
        {
            this.channel = conn.CreateModel();
            this.channel.QueueBind(group??string.Empty, channel, string.Empty);
            var consumer = new EventingBasicConsumer(this.channel);
            consumer.Received+=(sender, @event) =>
            {
                messageRecieved(new(
                    @event.BasicProperties.MessageId,
                    @event.BasicProperties.Type,
                    @event.RoutingKey,
                    new(@event.BasicProperties.Headers.Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value.ToString()!))),
                    @event.Body,
                    () =>
                    {
                        this.channel.BasicAck(@event.DeliveryTag, false);
                        return ValueTask.CompletedTask;
                    }
                ));
            };
        }

        public ValueTask EndAsync()
        {
            channel.Close();
            return ValueTask.CompletedTask;
        }
    }
}
