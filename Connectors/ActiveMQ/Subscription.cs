using Apache.NMS;

namespace MQContract.ActiveMQ;

internal class Subscription(Func<IMessage, TaskCompletionSource, ValueTask> messageReceived, Action<Exception> errorReceived, ConsumerInstance consumer) 
    : BaseLoopSubscription(errorReceived)
{
    protected override async ValueTask CleanupAsync()
    {
        await(consumer?.CloseAsync() ?? Task.CompletedTask).ConfigureAwait(false);
        consumer?.Dispose();
    }

    protected override async ValueTask RecieveMessageAsync(CancellationToken cancelToken)
    {
        var msg = await consumer.ReceiveAsync().WaitAsync(cancelToken);
        if (msg!=null)
        {
            var ackSource = new TaskCompletionSource();
            await Task.WhenAll(
                messageReceived(msg, ackSource).AsTask(),
                ackSource.Task
            );
        }
    }
}
