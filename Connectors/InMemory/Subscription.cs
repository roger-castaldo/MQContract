using MQContract.Interfaces.Service;
using System.Threading.Channels;

namespace MQContract.InMemory
{
    internal class Subscription(MessageGroup group, Func<InternalServiceMessage,ValueTask> messageRecieved) : IServiceSubscription
    {
        private readonly Channel<InternalServiceMessage> channel = group.Register();

        public void Start()
        {
            Task.Run(async () =>
            {
                while (await channel.Reader.WaitToReadAsync())
                {
                    var message = await channel.Reader.ReadAsync();
                    await messageRecieved(message); 
                }
            });
        }

        async ValueTask IServiceSubscription.EndAsync()
        {
            channel.Writer.TryComplete();
            await group.UnregisterAsync(channel);
        }
    }
}
