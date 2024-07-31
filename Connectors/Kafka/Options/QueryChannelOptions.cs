using MQContract.Interfaces.Service;

namespace MQContract.Kafka.Options
{
    public record QueryChannelOptions(string ReplyChannel) : IServiceChannelOptions
    {
    }
}
