namespace MQContract.CQRS.Interfaces.Command
{
    /// <summary>
    /// Used to identify a command type
    /// </summary>
    public interface ICommand { }

    /// <summary>
    /// Used to identify a command type that is expected to provide a response
    /// </summary>
    /// <typeparam name="TCommandResult">The type of response expected from this command</typeparam>
    public interface ICommand<TCommandResult> : ICommand { }
}
