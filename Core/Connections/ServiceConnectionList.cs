using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Connections
{
    internal class ServiceConnectionList : IDisposable
    {
        public record ServiceConnection(string ServiceConnectionName, IMessageServiceConnection MessageServiceConnection);
        private sealed record ServiceConnectionEntry(Func<(string channel, Type messageType, MessageHeader messageHeader), bool> CheckCallback, string ServiceConnectionName, IMessageServiceConnection MessageServiceConnection)
            : ServiceConnection(ServiceConnectionName,MessageServiceConnection);

        private readonly SemaphoreSlim dataLock = new(1, 1);
        private readonly List<ServiceConnectionEntry> connections = [];
        private bool disposedValue;

        public uint? MaxMessageBodySize { get; private set; } = null;

        public void Add(Func<(string channel, Type messageType, MessageHeader messageHeader), bool> checkCallback, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
        {
            dataLock.Wait();
            if (messageServiceConnection.MaxMessageBodySize!=null)
                MaxMessageBodySize = (MaxMessageBodySize==null ? messageServiceConnection.MaxMessageBodySize : Math.Min((uint)MaxMessageBodySize!, (uint)messageServiceConnection.MaxMessageBodySize!));
            connections.Add(new(checkCallback,serviceConnectionName, messageServiceConnection));
            dataLock.Release();
        }

        public async ValueTask<IEnumerable<ServiceConnection>> GetAsync(string channel,Type messageType,MessageHeader messageHeader)
        {
            await dataLock.WaitAsync();
            var results = connections.Where(conn => conn.CheckCallback((channel, messageType, messageHeader)))
                .OfType<ServiceConnection>()
                .DistinctBy(ss => ss.ServiceConnectionName)
                .ToArray();
            dataLock.Release();
            return results;
        }

        public IEnumerable<ServiceConnection> FullList
        {
            get
            {
                dataLock.Wait();
                var result = connections.DistinctBy(ss => ss.ServiceConnectionName).ToArray();
                dataLock.Release();
                return result;
            }
        }

        public async ValueTask CloseAsync()
        {
            await dataLock.WaitAsync();
            await connections.DistinctBy(sse => sse.ServiceConnectionName)
                .Select(sse => sse.MessageServiceConnection.CloseAsync())
                .WhenAll();
            connections.Clear();
            dataLock.Release();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    dataLock.Wait();
                    connections.Clear();
                    dataLock.Release();
                }
                disposedValue=true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ServiceConnectionList()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
