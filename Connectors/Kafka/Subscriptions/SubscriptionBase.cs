﻿using MQContract.Interfaces.Service;

namespace MQContract.Kafka.Subscriptions
{
    internal abstract class SubscriptionBase(Confluent.Kafka.IConsumer<string, byte[]> consumer,string channel) : IServiceSubscription
    {
        protected readonly Confluent.Kafka.IConsumer<string, byte[]> Consumer = consumer;
        protected readonly string Channel = channel;
        private bool disposedValue;
        protected readonly CancellationTokenSource cancelToken = new();

        protected abstract ValueTask RunAction();
        public Task Run()
        {
            var resultSource = new TaskCompletionSource();
            Task.Run(async () =>
            {
                Consumer.Subscribe(Channel);
                resultSource.TrySetResult();
                await RunAction();
                Consumer.Close();
            });
            return resultSource.Task;
        }

        public async ValueTask EndAsync()
        {
            try { await cancelToken.CancelAsync(); } catch { }
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
