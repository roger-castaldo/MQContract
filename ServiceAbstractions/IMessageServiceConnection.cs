using MQContract.ServiceAbstractions.Messages;

namespace MQContract.ServiceAbstractions
{
    public interface IMessageServiceConnection
    {
        int? MaxMessageBodySize { get; }
        TimeSpan DefaultTimout { get; }
        Task<IPingResult> PingAsync();
        Task<ITransmissionResult> PublishAsync(IServiceMessage message,TimeSpan timeout, IServiceChannelOptions? options=null, CancellationToken cancellationToken = new CancellationToken());
        Task<IServiceQueryResult> QueryAsync(IServiceMessage message, TimeSpan timeout, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken());
    }
}
