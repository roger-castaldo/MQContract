namespace MQContract.CQRS.Interfaces.Command
{
    public interface IFilteredCommandProcessor<TCommand> : ICommandProcessor<TCommand>
        where TCommand : ICommand
    {
        Func<TCommand, Context, ValueTask<MessageFilterResult>> Filter { get; }
    }
}
