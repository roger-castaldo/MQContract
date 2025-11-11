namespace MQContract.CQRS.Interfaces.Command
{
    public interface ICommandProcessor<TCommand> : IProcessor
        where TCommand : ICommand
    {
        ValueTask ProcessCommandAsync(ICommandInvocationContext<TCommand> invocationContext,CancellationToken cancellationToken);
    }

    public interface ICommandProcessor<TCommand, TCommandResult> : IProcessor
        where TCommand : ICommand<TCommandResult>
    {
        ValueTask<TCommandResult> ProcessCommandAsync(ICommandInvocationContext<TCommand> invocationContext, CancellationToken cancellationToken);
    }
}
