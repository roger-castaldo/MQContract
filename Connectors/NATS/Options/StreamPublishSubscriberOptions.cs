using MQContract.Interfaces.Service;
using NATS.Client.JetStream.Models;

namespace MQContract.NATS.Options
{
    public record StreamPublishSubscriberOptions(StreamConfig? StreamConfig=null,ConsumerConfig? ConsumerConfig=null) : IServiceChannelOptions
    {
    }
}
