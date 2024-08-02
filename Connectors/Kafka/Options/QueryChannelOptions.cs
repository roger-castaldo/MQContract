using MQContract.Interfaces.Service;

namespace MQContract.Kafka.Options
{
    /// <summary>
    /// Houses the QueryChannelOptions used for performing Query Response style messaging in Kafka
    /// </summary>
    /// <param name="ReplyChannel">The reply channel to use.  This channel should be setup with a short retention policy, no longer than 5 minutes.</param>
    public record QueryChannelOptions(string ReplyChannel) : IServiceChannelOptions
    {
    }
}
