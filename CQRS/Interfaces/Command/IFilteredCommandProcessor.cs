namespace MQContract.CQRS.Interfaces.Command;

/// <summary>
/// Used to define a command processor that will filter incoming messages based on the command
/// </summary>
/// <typeparam name="TCommand">The type of command that this processor handles</typeparam>
public interface IFilteredCommandProcessor<TCommand> : ICommandProcessor<TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// The filter callback expected to return a filter result and will be supplied the current 
    /// context and command instance
    /// </summary>
    Func<TCommand, Context, ValueTask<MessageFilterResult>> Filter { get; }
}
