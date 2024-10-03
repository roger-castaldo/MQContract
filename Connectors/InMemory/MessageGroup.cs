using System.Threading.Channels;

namespace MQContract.InMemory
{
    internal class MessageGroup(string group,Action<MessageGroup> removeMe) 
    {
        private readonly SemaphoreSlim semLock = new(1, 1);
        private readonly List<Channel<InternalServiceMessage>> channels = [];
        private int index = 0;
        public string Group => group;

        public Channel<InternalServiceMessage> Register()
        {
            var result = Channel.CreateUnbounded<InternalServiceMessage>(new UnboundedChannelOptions() { SingleReader=true,SingleWriter=true});
            channels.Add(result);
            return result;
        }

        public async ValueTask UnregisterAsync(Channel<InternalServiceMessage> channel)
        {
            await semLock.WaitAsync();
            channels.Remove(channel);
            if (channels.Count == 0)
                removeMe(this);
            semLock.Release();
        }

        public async ValueTask<bool> PublishMessage(InternalServiceMessage message)
        {
            var success = false;
            await semLock.WaitAsync();
            if (index>=channels.Count)
                index=0;
            if (index<channels.Count)
            {
                await channels[index].Writer.WriteAsync(message);
                index++;
                success=true;
            }
            semLock.Release();
            return success;
        }

        internal void Close()
        {
            semLock.Wait();
            foreach (var channel in channels)
                channel.Writer.TryComplete();
            channels.Clear();
            semLock.Release();
        }
    }
}
