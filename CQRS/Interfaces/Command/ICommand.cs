namespace MQContract.CQRS.Interfaces.Command
{
    public interface ICommand { }

    public interface ICommand<TCommandResult> : ICommand { }
}
