namespace MQContract.CQRS.Interfaces
{
    /// <summary>
    /// Used to define a processor that will filter incoming calls based on the context
    /// </summary>
    public interface IContextFilteredProcessor : IProcessor
    {
        /// <summary>
        /// The filter callback that will be supplied a context instance and will return a 
        /// filter type response
        /// </summary>
        Func<Context, ValueTask<MessageFilterResult>> Filter { get; }
    }
}
