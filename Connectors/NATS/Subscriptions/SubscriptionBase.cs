using MQContract.Messages;
using NATS.Client.Core;
using NATS.Client.JetStream;

namespace MQContract.NATS.Subscriptions
{
    internal abstract class SubscriptionBase() : IInternalServiceSubscription, IAsyncDisposable
    {
        private Task? runningTask;
        private readonly CancellationTokenSource CancelTokenSource = new();
        private bool disposedValue;

        protected CancellationToken CancelToken => CancelTokenSource.Token;

        protected static ReceivedServiceMessage ExtractMessage(INatsJSMsg<byte[]> receivedMessage, Func<ValueTask> acknowledge)
            => ExtractMessage(receivedMessage.Headers, receivedMessage.Subject, receivedMessage.Data, acknowledge);

        protected static ReceivedServiceMessage ExtractMessage(NatsMsg<byte[]> receivedMessage)
            => ExtractMessage(receivedMessage.Headers, receivedMessage.Subject, receivedMessage.Data);

        private static ReceivedServiceMessage ExtractMessage(NatsHeaders? headers, string subject, byte[]? data, Func<ValueTask>? acknowledge=null)
        {
            var convertedHeaders = Connection.ExtractHeader(headers, out var messageID, out var messageTypeID);
            return new(
                messageID??string.Empty,
                messageTypeID??string.Empty,
                subject,
                convertedHeaders,
                data??new ReadOnlyMemory<byte>(),
                acknowledge: acknowledge
            );
        }

        protected abstract Task RunAction();
        public void Run()
            => runningTask = RunAction();

        public ValueTask EndAsync()
            => ((IAsyncDisposable)this).DisposeAsync();

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                if (!CancelTokenSource.IsCancellationRequested)
                {
                    await CancelTokenSource.CancelAsync();
                    await(runningTask??Task.CompletedTask);
                }

                CancelTokenSource.Dispose();
                runningTask=null;
                GC.SuppressFinalize(this);
            }
        }
    }
}
