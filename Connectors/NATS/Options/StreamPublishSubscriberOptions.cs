using MQContract.Interfaces.Service;
using NATS.Client.JetStream.Models;

namespace MQContract.NATS.Options
{
    /// <summary>
    /// Used to specify when a subscription call is subscribing to a JetStream and not the standard subscription
    /// </summary>
    /// <param name="StreamConfig">The StreamConfig to use if not already defined</param>
    /// <param name="ConsumerConfig">The ConsumerCondig to use if specific settings are required</param>
    public record StreamPublishSubscriberOptions(StreamConfig? StreamConfig=null,ConsumerConfig? ConsumerConfig=null) : IServiceChannelOptions
    {
    }
}
