namespace MQContract.CQRS.Interfaces.Command
{
    /// <summary>
    /// Defines a command processor for the given type of command
    /// </summary>
    /// <example>
    /// <code>
    /// public class CreateUserProcessor : ICommandProcessor&lt;CreateUserCommand&gt;
    /// {
    ///     public async ValueTask ProcessCommandAsync(ICommandInvocationContext&lt;CreateUserCommand&gt; context, CancellationToken cancellationToken)
    ///     {
    ///         var command = context.Command;
    ///         // Process the command (e.g., save to database)
    ///         Console.WriteLine($"User created: {command.UserName}");
    ///     }
    /// 
    ///     void IProcessor.ErrorRecieved(Exception error)
    ///     {
    ///         Console.WriteLine($"Error: {error.Message}");
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="TCommand">The type of command</typeparam>
    public interface ICommandProcessor<TCommand> : IProcessor
        where TCommand : ICommand
    {
        /// <summary>
        /// The callback executed against the command
        /// </summary>
        /// <param name="invocationContext">The current invocation context for this command instance</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A ValueTask for async purposes</returns>
        ValueTask ProcessCommandAsync(ICommandInvocationContext<TCommand> invocationContext,CancellationToken cancellationToken);
    }

    /// <summary>
    /// Defines a command processor for the given type of command that expects a response
    /// </summary>
    /// <typeparam name="TCommand">The type of command</typeparam>
    /// <typeparam name="TCommandResult">The type of response from the command</typeparam>
    public interface ICommandProcessor<TCommand, TCommandResult> : IProcessor
        where TCommand : ICommand<TCommandResult>
    {
        /// <summary>
        /// The callback executed against the command
        /// </summary>
        /// <param name="invocationContext">The current invocation context for this command instance</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The result from the command execution</returns>
        ValueTask<TCommandResult> ProcessCommandAsync(ICommandInvocationContext<TCommand> invocationContext, CancellationToken cancellationToken);
    }
}
