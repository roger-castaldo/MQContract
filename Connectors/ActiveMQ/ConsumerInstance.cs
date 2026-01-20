using Apache.NMS;

namespace MQContract.ActiveMQ
{
    internal sealed class ConsumerInstance(IMessageConsumer messageConsumer, Action cleanup)
        : IDisposable
    {
        private int listenerCount = 1;
        private bool disposedValue;

        public void AddListener() => listenerCount++;

        public Task<IMessage> ReceiveAsync() => messageConsumer.ReceiveAsync();

        public async Task CloseAsync()
        {
            listenerCount--;
            if (listenerCount == 0)
            {
                await messageConsumer.CloseAsync();
                cleanup();
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    messageConsumer.Dispose();
                }
                disposedValue=true;
            }
        }

        ~ConsumerInstance()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
