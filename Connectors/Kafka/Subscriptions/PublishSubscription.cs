using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Kafka.Subscriptions
{
    internal class PublishSubscription(Confluent.Kafka.IConsumer<string, byte[]> consumer, Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel)
        : IServiceSubscription
    {
        private bool disposedValue;
        protected readonly CancellationTokenSource cancelToken = new();

        public void Start()
        {
            Task.Run(() =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    System.Diagnostics.Debug.WriteLine($"Waiting to consume message from {channel}");
                    try
                    {
                        var msg = consumer.Consume(cancellationToken: cancelToken.Token);
                        System.Diagnostics.Debug.WriteLine($"Consuming message {msg.Offset} from {channel}");
                        var headers = Connection.ExtractHeaders(msg.Message.Headers, out var messageTypeID);
                        messageReceived(new ReceivedServiceMessage(
                            msg.Message.Key??string.Empty,
                            messageTypeID??string.Empty,
                            channel,
                            headers,
                            msg.Message.Value
                        ));
                    }
                    catch (OperationCanceledException)
                    {
                        //dropped this exception as it can occur when the consumption is stopped
                    }
                    catch (Exception ex)
                    {
                        errorReceived(ex);
                    }
                }
                consumer.Close();
            });
        }

        public async ValueTask EndAsync()
        {
            try { 
                await cancelToken.CancelAsync(); 
            } 
            catch {
                //ignoring the error as the goal is to call cancel and not care about the error
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                if (!cancelToken.IsCancellationRequested)
                    await cancelToken.CancelAsync();
                try
                {
                    consumer.Close();
                }
                catch {
                    //ignoring error here as we are attempting to dispose the resource
                }
                consumer.Dispose();
                cancelToken.Dispose();
            }
        }
    }
}
