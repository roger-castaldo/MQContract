using Amazon.SQS;
using Amazon.SQS.Model;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.AmazonSNQS
{
    internal class Subscription(AmazonSQSClient sqsClient, string queueUrl, Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, CancellationToken connectionCancellationToken)
        : IServiceSubscription, IAsyncDisposable
    {
        protected readonly CancellationTokenSource cancelToken = new();
        private bool disposedValue;

        public void Start()
        {
            connectionCancellationToken.Register(() =>
            {
                try
                {
                    if (!cancelToken.IsCancellationRequested)
                        cancelToken.Cancel();
                }
                catch
                {
                    //exception is ignored here because the cancellation token may have been disposed of already
                }
            });
            _ = Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var receiveResponse = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                        {
                            QueueUrl = queueUrl,
                            MaxNumberOfMessages = 1,
                            WaitTimeSeconds = 5
                        }, cancelToken.Token);

                        foreach (var msg in receiveResponse.Messages?? [])
                        {
                            await messageReceived(MessageMapper.Map(msg, async () =>
                            {
                                await sqsClient.DeleteMessageAsync(queueUrl, msg.ReceiptHandle);
                            })).ConfigureAwait(false);
                        }
                    }
                    catch (Exception error)
                    {
                        errorReceived(error);
                    }
                }
            });
        }

        async ValueTask IServiceSubscription.EndAsync()
        {
            if (!cancelToken.IsCancellationRequested)
                await cancelToken.CancelAsync();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                await ((IServiceSubscription)this).EndAsync();
                cancelToken.Dispose();
            }
        }
    }
}
