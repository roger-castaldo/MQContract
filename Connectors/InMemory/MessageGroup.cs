using System.Collections.Concurrent;
using System.Threading.Channels;

namespace MQContract.InMemory;

internal class MessageGroup(Action removeMe)
{
    private readonly ConcurrentDictionary<Guid, Channel<InternalServiceMessage>> channels = [];
    private int index = 0;

    public (Guid id, Channel<InternalServiceMessage> channel) Register()
    {
        var id = Guid.NewGuid();
        var result = Channel.CreateUnbounded<InternalServiceMessage>(new UnboundedChannelOptions() { SingleReader=true, SingleWriter=true });
        channels.TryAdd(id, result);
        return (id, result);
    }

    public void Unregister(Guid id)
    {
        channels.TryRemove(id, out _);
        if (channels.IsEmpty)
            removeMe();
    }

    public async ValueTask PublishMessageAsync(InternalServiceMessage message, CancellationToken cancellationToken)
    {
        if (index>=channels.Count)
            index=0;
        if (index<channels.Count)
        {
            var key = channels.Keys.ElementAt(index);
            await channels[key].Writer.WriteAsync(message, cancellationToken);
            index++;
        }
    }

    internal void Close()
    {
        var channelsToClose = channels.Values.ToArray();
        foreach (var channel in channelsToClose)
            channel.Writer.TryComplete();
        channels.Clear();
    }
}
