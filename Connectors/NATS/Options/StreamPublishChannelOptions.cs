using MQContract.Interfaces.Service;
using NATS.Client.JetStream.Models;

namespace MQContract.NATS.Options
{
    public record StreamPublishChannelOptions(StreamConfig? Config=null) : IServiceChannelOptions
    {
    }
}
