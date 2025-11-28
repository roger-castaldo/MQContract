using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace BenchMark.PublishBenchmarks

{
    internal class FakePublishConnection : IMessageServiceConnection
    {
        uint? IMessageServiceConnection.MaxMessageBodySize => 1024 * 1024;

        ValueTask IMessageServiceConnection.CloseAsync()
            => ValueTask.CompletedTask;

        ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
            => ValueTask.FromResult<TransmissionResult>(new(message.ID));

        ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
