using NATS.Client.JetStream.Models;

namespace MQContract.NATS.Options
{
    internal record SubscriptionConsumerConfig(string Channel,ConsumerConfig Configuration)
    {
    }
}
