namespace MQContract.Interfaces.Middleware
{
    /// <summary>
    /// Used to define a MessageContext aware middleware that will handle message contexts being registered and perform actions based on them.
    /// </summary>
    public interface IMessageContextAwareMiddleware : IMiddleware
    {
        /// <summary>
        /// Called when a MessageContext is registered within the system.
        /// </summary>
        /// <param name="messages">The messages that a particular MessageContext defines</param>
        /// <returns></returns>
        ValueTask ProcessMessagesFromMessageContextAsync(IEnumerable<MessageContextDefintion> messages);
    }
}
