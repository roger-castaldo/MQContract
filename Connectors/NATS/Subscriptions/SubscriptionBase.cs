using MQContract.Messages;
using NATS.Client.Core;
using NATS.Client.JetStream;

namespace MQContract.NATS.Subscriptions
{
    internal abstract class SubscriptionBase() : IInternalServiceSubscription, IDisposable
    {
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
                Acknowledge: acknowledge
            );
        }

        protected abstract Task RunAction();
        public void Run()
            => RunAction();

        public async ValueTask EndAsync()
        {
            if (!CancelTokenSource.IsCancellationRequested)
                await CancelTokenSource.CancelAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing&&!CancelTokenSource.IsCancellationRequested)
                    CancelTokenSource.Cancel();

                CancelTokenSource.Dispose();
                disposedValue=true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
