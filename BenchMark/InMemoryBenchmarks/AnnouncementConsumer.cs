using BenchMark.Messages;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;

namespace BenchMark.InMemoryBenchmarks
{
    internal class AnnouncementConsumer(int count, TaskCompletionSource completionSource)
        : IPubSubAsyncConsumer<Announcement>
    {
        void IBaseConsumer.ErrorRecieved(Exception error)
        {
        }

        ValueTask IPubSubAsyncConsumer<Announcement>.MessageReceivedAsync(IReceivedMessage<Announcement> message)
        {
            count--;
            if (count<=0)
                completionSource.TrySetResult();
            return ValueTask.CompletedTask;
        }
    }
}
