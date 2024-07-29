using MQContract.Messages;

namespace MQContract.Interfaces.Service
{
    public interface IMessageServiceConnection
    {
        int? MaxMessageBodySize { get; }
        TimeSpan DefaultTimout { get; }
        Task<PingResult> PingAsync();
        Task<TransmissionResult> PublishAsync(ServiceMessage message, TimeSpan timeout, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken());
        Task<IServiceSubscription?> SubscribeAsync(Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken());
        Task<ServiceQueryResult> QueryAsync(ServiceMessage message, TimeSpan timeout, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken());
        Task<IServiceSubscription?> SubscribeQueryAsync(Func<RecievedServiceMessage, Task<ServiceMessage>> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken());
    }
}
