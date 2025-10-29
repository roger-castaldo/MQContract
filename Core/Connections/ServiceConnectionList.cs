using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Connections
{
    internal class ServiceConnectionList : IAsyncDisposable
    {
        public record ServiceConnection(string ServiceConnectionName, IMessageServiceConnection MessageServiceConnection);
        private sealed record ServiceConnectionEntry(Func<(string channel, Type messageType, MessageHeader messageHeader), bool> CheckCallback, string ServiceConnectionName, IMessageServiceConnection MessageServiceConnection)
            : ServiceConnection(ServiceConnectionName, MessageServiceConnection);

        private readonly SemaphoreSlim dataLock = new(1, 1);
        private readonly List<ServiceConnectionEntry> connections = [];
        private bool disposedValue;

        public uint? MaxMessageBodySize { get; private set; } = null;

        public void Add(Func<(string channel, Type messageType, MessageHeader messageHeader), bool> checkCallback, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
        {
            dataLock.Wait();
            if (messageServiceConnection.MaxMessageBodySize!=null)
                MaxMessageBodySize = (MaxMessageBodySize==null ? messageServiceConnection.MaxMessageBodySize : Math.Min((uint)MaxMessageBodySize!, (uint)messageServiceConnection.MaxMessageBodySize!));
            connections.Add(new(checkCallback, serviceConnectionName, messageServiceConnection));
            dataLock.Release();
        }

        public async ValueTask<IEnumerable<ServiceConnection>> GetAsync(string channel, Type messageType, MessageHeader messageHeader)
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

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                await dataLock.WaitAsync();
                var conns = connections.DistinctBy(ss => ss.ServiceConnectionName).ToArray();
                await Task.WhenAll(conns.Select(async conn =>
                {
                    if (conn.MessageServiceConnection is IAsyncDisposable asyncDisposable)
                        await asyncDisposable.DisposeAsync();
                    else if (conn.MessageServiceConnection is IDisposable disposable)
                        disposable.Dispose();
                }));
                connections.Clear();
                dataLock.Release();
                dataLock.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
