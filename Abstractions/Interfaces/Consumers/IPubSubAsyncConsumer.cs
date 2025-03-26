namespace MQContract.Interfaces.Consumers
{
    /// <summary>
    /// Represents an Asynchronous PubSub Message Consumer to be registered to the ContractConnection
    /// </summary>
    /// <typeparam name="T">The type of Message that this Consumer will consume</typeparam>
    public interface IPubSubAsyncConsumer<in T> : IBaseConsumer
    {
        /// <summary>
        /// Called when a message is recieved from the underlying subscription that is using this Consumer
        /// </summary>
        /// <param name="message">The message that was received</param>
        /// <returns>A ValueTask for asynchronous operations</returns>
        ValueTask MessageReceivedAsync(IReceivedMessage<T> message);

    }
}
