using System.Collections.Concurrent;
using System.Threading.Channels;

namespace MQContract.InMemory
{
    internal class MessageGroup(Action removeMe) : IDisposable
    {
        private readonly ConcurrentDictionary<Guid, Channel<InternalServiceMessage>> channels = [];
        private readonly SemaphoreSlim semaphore = new(1, 1);
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

        public async ValueTask<IEnumerable<bool>> PublishMessagesAsync(IEnumerable<InternalServiceMessage> messages, CancellationToken cancellationToken)
        {
            if (channels.IsEmpty)
                return messages.Select(m => false);
            await semaphore.WaitAsync(cancellationToken);
            var results = new List<bool>();
            foreach (var message in messages)
            {
                try
                {
                    if (index>=channels.Count)
                        index=0;
                    if (index<channels.Count)
                    {
                        var key = channels.Keys.ElementAt(index);
                        await channels[key].Writer.WriteAsync(message, cancellationToken);
                        index++;
                        results.Add(true);
                    }
                    else
                        results.Add(false);
                }
                catch
                {
                    results.Add(false);
                    continue;
                }
            }
            semaphore.Release();
            return results;
        }

        internal void Close()
        {
            var channelsToClose = channels.Values.ToArray();
            foreach (var channel in channelsToClose)
                channel.Writer.TryComplete();
            channels.Clear();
        }

        public void Dispose()
        {
            ((IDisposable)semaphore).Dispose();
        }
    }
}
