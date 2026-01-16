using Confluent.Kafka;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Diagnostics;

namespace MQContract.Kafka.Subscriptions
{
    internal class PublishSubscription(Confluent.Kafka.IConsumer<string, byte[]> consumer, Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel)
        : IServiceSubscription
    {
        private bool disposedValue;
        protected readonly CancellationTokenSource cancelToken = new();

        public void Start()
        {
            Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var msg = consumer.Consume(TimeSpan.FromMinutes(5));
                        if (msg==null && !cancelToken.IsCancellationRequested)
                            msg = consumer.Consume(cancellationToken: cancelToken.Token);
                        if (msg!=null)
                        {
                            Activity.Current?.AddEvent(new("Message received from Kafka"));
                            var headers = Connection.ExtractHeaders(msg.Message.Headers, out var messageTypeID);
                            Activity.Current?.AddEvent(new("Invoking Message Callback"));
                            await messageReceived(new ReceivedServiceMessage(
                                msg.Message.Key ?? string.Empty,
                                messageTypeID ?? string.Empty,
                                channel,
                                headers,
                                msg.Message.Value,
                                Acknowledge: () =>
                                {
                                    consumer.StoreOffset(msg.TopicPartitionOffset);
                                    consumer.Commit();
                                    return ValueTask.CompletedTask;
                                }
                            )).ConfigureAwait(false);
                        }
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
