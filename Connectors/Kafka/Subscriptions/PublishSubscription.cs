using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Kafka.Subscriptions
{
    internal class PublishSubscription(Confluent.Kafka.IConsumer<string, byte[]> consumer, Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel)
        : IServiceSubscription, IAsyncDisposable
    {
        private bool disposedValue;
        protected readonly CancellationTokenSource cancelToken = new();
        private Task? consumerLoop;

        public void Start()
        {
            consumerLoop = Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var msg = consumer.Consume(cancellationToken: cancelToken.Token);
                        var ackSource = new TaskCompletionSource();
                        var headers = Connection.ExtractHeaders(msg.Message.Headers, out var messageTypeID);
                        _ = await Task.WhenAny(
                            messageReceived(new ReceivedServiceMessage(
                                msg.Message.Key??string.Empty,
                                messageTypeID??string.Empty,
                                channel,
                                headers,
                                msg.Message.Value,
                                acknowledge: async () =>
                                {
                                    consumer.Commit(msg);
                                    ackSource.TrySetResult();
                                }
                            )).AsTask(),
                            ackSource.Task
                        );
                    }
                    catch (OperationCanceledException)
                    {
                        //dropped this exception as it can occur when the consumption is stopped
                    }
                    catch (AccessViolationException)
                    {
                        //dropped this exception as it can occur when the consumption is stopped
                    }
                    catch (Exception ex)
                    {
                        errorReceived(ex);
                    }
                }
                try
                {
                    consumer.Close();
                }
                catch
                {
                    //ignoring error here as we are attempting to dispose the resource
                }
            });
        }

        public async ValueTask EndAsync()
            => await DisposeAsync();

        public async ValueTask DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                if (!cancelToken.IsCancellationRequested)
                {
                    cancelToken.Cancel();
                    try
                    {
                        await consumerLoop!.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) { }
                }
                consumer.Dispose();
                cancelToken.Dispose();
            }
        }
    }
}
