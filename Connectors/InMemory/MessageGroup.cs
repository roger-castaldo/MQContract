using System.Threading.Channels;

namespace MQContract.InMemory
{
    internal class MessageGroup(string group, Action<MessageGroup> removeMe)
    {
        private readonly ReaderWriterLockSlim locker = new();
        private readonly List<Channel<InternalServiceMessage>> channels = [];
        private int index = 0;
        public string Group => group;

        public Channel<InternalServiceMessage> Register()
        {
            var result = Channel.CreateUnbounded<InternalServiceMessage>(new UnboundedChannelOptions() { SingleReader=true, SingleWriter=true });
            channels.Add(result);
            return result;
        }

        public ValueTask UnregisterAsync(Channel<InternalServiceMessage> channel)
        {
            locker.EnterWriteLock();
            channels.Remove(channel);
            if (channels.Count == 0)
                removeMe(this);
            locker.ExitWriteLock();
            return ValueTask.CompletedTask;
        }

        public async ValueTask<bool> PublishMessageAsync(InternalServiceMessage message,CancellationToken cancellationToken)
        {
            var success = false;
            locker.EnterReadLock();
            if (index>=channels.Count)
                index=0;
            if (index<channels.Count)
            {
                await channels[index].Writer.WriteAsync(message,cancellationToken);
                index++;
                success=true;
            }
            locker.ExitReadLock();
            return success;
        }

        internal void Close()
        {
            locker.EnterWriteLock();
            var channelsToClose = channels.ToArray();
            channels.Clear();
            locker.ExitWriteLock();
            foreach (var channel in channelsToClose)
                channel.Writer.TryComplete();
        }
    }
}
