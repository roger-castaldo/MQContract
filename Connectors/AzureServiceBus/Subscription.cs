using Azure.Messaging.ServiceBus;

namespace MQContract.AzureServiceBus;

internal class Subscription(ServiceBusClient client, Func<ServiceBusReceivedMessage, Func<Task>, ValueTask> messageRecieved, Action<Exception> errorRecieved, string channel, string? group, string? sessionId = null)
    : BaseLoopSubscription(errorRecieved)
{
    private ServiceBusReceiver? receiver;

    protected override async ValueTask PreStartAsync()
    {
        receiver = (sessionId==null ? client.CreateReceiver(channel, group??channel) : await client.AcceptSessionAsync(channel, group??channel, sessionId));
    }

    protected override async ValueTask RecieveMessageAsync(CancellationToken cancelToken)
    {
        var msg = await receiver!.ReceiveMessageAsync(cancellationToken: cancelToken);
        if (msg!=null)
        {
            var ackSource = new TaskCompletionSource();
            await Task.WhenAny(
                messageRecieved(msg, async () =>
                {
                    await receiver.CompleteMessageAsync(msg);
                    ackSource.SetResult();
                }).AsTask(),
                ackSource.Task
            );
        }
    }

    protected override async ValueTask CleanupAsync()
    {
        await receiver!.CloseAsync();
        await receiver!.DisposeAsync();
    }
}
