using MQContract.Interfaces.Service;

namespace MQContract.NATS.Subscriptions
{
    internal abstract class SubscriptionBase(Confluent.Kafka.IConsumer<string, byte[]> consumer,string channel,CancellationToken cancellationToken) : IServiceSubscription
    {
        protected readonly Confluent.Kafka.IConsumer<string, byte[]> Consumer = consumer;
        protected readonly string Channel = channel;
        private bool disposedValue;
        protected readonly CancellationTokenSource cancelToken = new();

        protected abstract Task RunAction();
        public void Run()
        {
            cancellationToken.Register(() =>
            {
                cancelToken.Cancel();
            });
            Task.Run(async () =>
            {
                Consumer.Subscribe(Channel);
                await RunAction();
                Consumer.Close();
            });
        }

        public async Task EndAsync()
        {
            try { await cancelToken.CancelAsync(); } catch { }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (!cancellationToken.IsCancellationRequested) 
                        cancelToken.Cancel();
                    try
                    {
                        Consumer.Close();
                    }
                    catch (Exception) { }
                    Consumer.Dispose();
                    cancelToken.Dispose();
                }
                disposedValue=true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
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
                    Consumer.Close();
                }
                catch (Exception) { }
                Consumer.Dispose();
                cancelToken.Dispose();
            }
        }
    }
}
