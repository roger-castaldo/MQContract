using MQContract.Interfaces.Service;

namespace MQContract.KubeMQ.Options
{
    /// <summary>
    /// Houses the Publish Channel options used when calling the Publish command
    /// </summary>
    /// <param name="Stored">Indicates if the publish should be using storage</param>
    public record PublishChannelOptions(bool Stored) : IServiceChannelOptions
    {
    }
}
