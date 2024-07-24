using MQContract.Interfaces.Service;

namespace MQContract.KubeMQ.Options
{
    public record PublishChannelOptions(bool Stored) : IServiceChannelOptions
    {
    }
}
