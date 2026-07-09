using Amazon.SQS;
using Amazon.SQS.Model;
using MQContract.Messages;

namespace MQContract.AmazonSNQS;

internal class Subscription(AmazonSQSClient sqsClient, string queueUrl, Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, CancellationToken connectionCancellationToken)
    : BaseLoopSubscription(errorReceived)
{
    protected override async ValueTask RecieveMessageAsync(CancellationToken cancelToken)
    {
        var receiveResponse = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = 1,
            WaitTimeSeconds = 5
        }, cancelToken);

        foreach (var msg in receiveResponse.Messages?? [])
        {
            var ackSource = new TaskCompletionSource();
            await Task.WhenAny(
                messageReceived(MessageMapper.Map(msg, async () =>
                {
                    await sqsClient.DeleteMessageAsync(queueUrl, msg.ReceiptHandle);
                    ackSource.TrySetResult();
                })).AsTask(),
                ackSource.Task
            );
        }
    }
    protected override void PreStart()
    {
        connectionCancellationToken.Register(() =>
        {
            try
            {
                if (!this.cancelToken.IsCancellationRequested)
                    this.cancelToken.Cancel();
            }
            catch
            {
                //exception is ignored here because the cancellation token may have been disposed of already
            }
        });
    }
}
