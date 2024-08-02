using MQContract.Interfaces.Service;
using NATS.Client.JetStream.Models;

namespace MQContract.NATS.Options
{
    /// <summary>
    /// Used to specify when a publish call is publishing to a JetStream
    /// </summary>
    /// <param name="Config">The StreamConfig to use if not already defined</param>
    public record StreamPublishChannelOptions(StreamConfig? Config=null) : IServiceChannelOptions
    {
    }
}
