namespace MQContract.Interfaces.Middleware
{
    /// <summary>
    /// This interface represents a Middleware to execute Before a message is encoded
    /// </summary>
    public interface IBeforeEncodeMiddleware : IMiddleware
    {
        /// <summary>
        /// This is the method invoked as part of the Middle Ware processing during message encoding
        /// </summary>
        /// <typeparam name="TMessage">The type of message being processed</typeparam>
        /// <param name="context">A shared context that exists from the start of this encoding instance</param>
        /// <param name="message">The message being encoded including headers and channel</param>
        /// <returns>The message, channel and header to allow for changes if desired</returns>
        ValueTask<EncodableMessage<TMessage>> BeforeMessageEncodeAsync<TMessage>(IContext context, EncodableMessage<TMessage> message);
    }
}
