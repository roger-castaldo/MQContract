namespace MQContract.CQRS.Interfaces.Command
{
    /// <summary>
    /// Represents a given execution context for a command
    /// </summary>
    /// <typeparam name="TCommand">The type of command housed within this context</typeparam>
    public interface ICommandInvocationContext<TCommand> : IInvocationContext
        where TCommand : ICommand
    {
        /// <summary>
        /// The command for this invocation context instance
        /// </summary>
        TCommand Command { get; }
    }
}
