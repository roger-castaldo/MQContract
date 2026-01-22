using MQContract.Interfaces.Service;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MQContract.RabbitMQ
{
    internal class Subscription : IServiceSubscription
    {
        private readonly IChannel channel;
        private readonly string consumerTag;

        public static async ValueTask<Subscription> ProduceInstanceAsync(IConnection conn, string channel, string group, Func<BasicDeliverEventArgs, IChannel, Func<ValueTask>, ValueTask> messageReceived, Action<Exception> errorReceived, string? routingKey = null)
        {
            var connectionChannel = await conn.CreateChannelAsync();
            await connectionChannel.QueueBindAsync(group, channel, routingKey??Guid.NewGuid().ToString());
            await connectionChannel.BasicQosAsync(0, 1, false);
            var consumer = new AsyncEventingBasicConsumer(connectionChannel);
            consumer.ReceivedAsync+= async (sender, @event) =>
            {
                var ackSource = new TaskCompletionSource();
                await Task.WhenAny(
                    messageReceived(
                        @event,
                        connectionChannel,
                        async () =>
                        {
                            await connectionChannel.BasicAckAsync(@event.DeliveryTag, false);
                            ackSource.TrySetResult();
                        }
                    ).AsTask(),
                    ackSource.Task
                );
            };

            return new Subscription(connectionChannel, await connectionChannel.BasicConsumeAsync(group, false, consumer));
        }

        private Subscription(IChannel channel, string consumerTag)
        {
            this.channel=channel;
            this.consumerTag=consumerTag;
        }

        public async ValueTask EndAsync()
        {
            await channel.BasicCancelAsync(consumerTag);
            await channel.CloseAsync();
        }
    }
}
