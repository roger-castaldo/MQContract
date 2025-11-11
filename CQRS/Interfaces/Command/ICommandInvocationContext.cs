namespace MQContract.CQRS.Interfaces.Command
{
    public interface ICommandInvocationContext<C> : IInvocationContext
        where C : ICommand
    {
        C Command { get; }
    }
}
