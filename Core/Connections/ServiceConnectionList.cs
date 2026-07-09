using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Collections.Concurrent;

namespace MQContract.Connections;

internal class ServiceConnectionList : IAsyncDisposable
{
    public record ServiceConnection(string ServiceConnectionName, IMessageServiceConnection MessageServiceConnection);

    private sealed record ServiceConnectionEntry(Func<(string channel, Type messageType, MessageHeader messageHeader), bool> CheckCallback, string ServiceConnectionName, IMessageServiceConnection MessageServiceConnection)
        : ServiceConnection(ServiceConnectionName, MessageServiceConnection), IAsyncDisposable
    {
        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (MessageServiceConnection is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else if (MessageServiceConnection is IDisposable disposable)
                disposable.Dispose();
        }
    }

    private readonly ConcurrentBag<ServiceConnectionEntry> connections = [];
    private bool disposedValue;

    public uint? MaxMessageBodySize { get; private set; } = null;

    public void Add(Func<(string channel, Type messageType, MessageHeader messageHeader), bool> checkCallback, string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
    {
        if (messageServiceConnection.MaxMessageBodySize!=null)
            MaxMessageBodySize = (MaxMessageBodySize==null ? messageServiceConnection.MaxMessageBodySize : Math.Min((uint)MaxMessageBodySize!, (uint)messageServiceConnection.MaxMessageBodySize!));
        connections.Add(new(checkCallback, serviceConnectionName, messageServiceConnection));
    }

    public async ValueTask<IEnumerable<ServiceConnection>> GetAsync(string channel, Type messageType, MessageHeader messageHeader)
        => [.. connections.Where(conn => conn.CheckCallback((channel, messageType, messageHeader)))
            .OfType<ServiceConnection>()
            .DistinctBy(ss => ss.ServiceConnectionName)];

    public IEnumerable<ServiceConnection> FullList => [.. connections.DistinctBy(ss => ss.ServiceConnectionName)];
    public async ValueTask CloseAsync()
    {
        await connections.DistinctBy(sse => sse.ServiceConnectionName)
            .Select(sse => sse.MessageServiceConnection.CloseAsync())
            .WhenAll();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (!disposedValue)
        {
            disposedValue=true;
            await Task.WhenAll(connections
                .DistinctBy(ss => ss.ServiceConnectionName)
                .Select(ss => ((IAsyncDisposable)ss).DisposeAsync().AsTask()));
            connections.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
