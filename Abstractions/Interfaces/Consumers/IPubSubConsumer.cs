namespace MQContract.Interfaces.Consumers
{
    /// <summary>
    /// Represents a PubSub Message Consumer to be registered to the ContractConnection
    /// </summary>
    /// <typeparam name="TMessage">The type of Message that this Consumer will consume</typeparam>
    public interface IPubSubConsumer<in TMessage> : IBaseConsumer
    {
        /// <summary>
        /// Called when a message is recieved from the underlying subscription that is using this Consumer
        /// </summary>
        /// <param name="message">The message that was received</param>
        void MessageReceived(IReceivedMessage<TMessage> message);
    }
}
