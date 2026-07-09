namespace MQContract.Interfaces.Middleware;

/// <summary>
/// This interface represents a Middleware to execute before decoding a ServiceMessage
/// </summary>
public interface IBeforeDecodeMiddleware : IMiddleware
{
    /// <summary>
    /// This is the method invoked as part of the Middleware processing prior to the message decoding
    /// </summary>
    /// <param name="context">A shared context that exists from the start of this decode process instance</param>
    /// <param name="id">The id of the message</param>
    /// <param name="messageTypeID">The message type id</param>
    /// <param name="messageChannel">The channel the message was recieved on</param>
    /// <param name="message">The decodable message housing headers and the data</param>
    /// <returns>The message header and data to allow for changes if desired</returns>
    ValueTask<DecodableMessage> BeforeMessageDecodeAsync(IContext context, string id, string messageTypeID, string messageChannel, DecodableMessage message);
}
