using BenchMark.Messages;
using MQContract.CQRS.Interfaces;
using MQContract.CQRS.Interfaces.Command;

namespace BenchMark.InMemoryBenchmarks
{
    internal class AnnouncementCommandProcessor(int count, TaskCompletionSource completionSource) : ICommandProcessor<AnnouncementCommand>
    {
        void IProcessor.ErrorRecieved(Exception error)
        {
        }

        ValueTask ICommandProcessor<AnnouncementCommand>.ProcessCommandAsync(ICommandInvocationContext<AnnouncementCommand> invocationContext, CancellationToken cancellationToken)
        {
            count--;
            if (count<=0)
                completionSource.TrySetResult();
            return ValueTask.CompletedTask;
        }
    }
}
