using MQContract.Messages;

namespace MQContract.Interfaces.Middleware
{
    /// <summary>
    /// This interface represents a Middleware to execute after a Message has been encoded to a ServiceMessage from the supplied Class
    /// </summary>
    public interface IAfterEncodeMiddleware : IMiddleware
    {
        /// <summary>
        /// This is the method invoked as part of the Middleware processing during message encoding
        /// </summary>
        /// <param name="messageType">The class of the message type that was encoded</param>
        /// <param name="context">A shared context that exists from the start of this encode process instance</param>
        /// <param name="message">The resulting encoded message</param>
        /// <returns>The message to allow for changes if desired</returns>
        ValueTask<ServiceMessage> AfterMessageEncodeAsync(Type messageType, IContext context, ServiceMessage message);
    }
}
