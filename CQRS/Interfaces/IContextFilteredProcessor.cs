namespace MQContract.CQRS.Interfaces
{
    public interface IContextFilteredProcessor : IProcessor
    {
        Func<Context, ValueTask<MessageFilterResult>> Filter { get; }
    }
}
