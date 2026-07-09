using MQContract.Interfaces.Service;
using System.Threading.Channels;

namespace MQContract.InMemory;

internal class Subscription(MessageGroup group, Func<InternalServiceMessage, ValueTask> messageRecieved) : IServiceSubscription
{
    private readonly (Guid id, Channel<InternalServiceMessage> channel) registration = group.Register();

    public void Start()
    {
        Task.Run(async () =>
        {
            while (await registration.channel.Reader.WaitToReadAsync())
            {
                var message = await registration.channel.Reader.ReadAsync();
                await messageRecieved(message).ConfigureAwait(false);
            }
        });
    }

    ValueTask IServiceSubscription.EndAsync()
    {
        registration.channel.Writer.TryComplete();
        group.Unregister(registration.id);
        return ValueTask.CompletedTask;
    }
}
