using MQContract.Messages;
using NATS.Client.Core;
using NATS.Client.JetStream;

namespace MQContract.NATS.Subscriptions
{
    internal abstract class SubscriptionBase(CancellationToken cancellationToken) : IInternalServiceSubscription
    {
        private bool disposedValue;
        protected readonly CancellationTokenSource cancelToken = new();

        protected static RecievedServiceMessage ExtractMessage(NatsJSMsg<byte[]> recievedMessage)
            => ExtractMessage(recievedMessage.Headers, recievedMessage.Subject, recievedMessage.Data);

        protected static RecievedServiceMessage ExtractMessage(NatsMsg<byte[]> recievedMessage)
            => ExtractMessage(recievedMessage.Headers, recievedMessage.Subject, recievedMessage.Data);

        private static RecievedServiceMessage ExtractMessage(NatsHeaders? headers, string subject, byte[]? data)
        {
            var convertedHeaders = Connection.ExtractHeader(headers, out var messageID, out var messageTypeID);
            return new(
                messageID??string.Empty,
                messageTypeID??string.Empty,
                subject,
                convertedHeaders,
                data??new ReadOnlyMemory<byte>()
            );
        }

        protected abstract Task RunAction();
        public void Run()
        {
            cancellationToken.Register(() =>
            {
                cancelToken.Cancel();
            });
            RunAction();
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
                    cancelToken.Cancel();
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
    }
}
