namespace MQContract.Interfaces.Middleware;

/// <summary>
/// This interface represents a Middleware to execute after a Message of the given type T has been decoded from a ServiceMessage to the expected Class
/// </summary>
public interface IAfterDecodeSpecificTypeMiddleware<TMessage> : ISpecificTypeMiddleware<TMessage>
{
    /// <summary>
    /// This is the method invoked as part of the Middleware processing during message decoding
    /// </summary>
    /// <param name="context">A shared context that exists from the start of this decode process instance</param>
    /// <param name="ID">The id of the message</param>
    /// <param name="message">The decoded message including the headers</param>
    /// <param name="receivedTimestamp">The timestamp of when the message was recieved</param>
    /// <param name="processedTimeStamp">The timestamp of when the message was decoded into a Class</param>
    /// <returns>The message and header to allow for changes if desired</returns>
    ValueTask<DecodedMessage<TMessage>> AfterMessageDecodeAsync(IContext context, string ID, DecodedMessage<TMessage> message, DateTime receivedTimestamp, DateTime processedTimeStamp);
}
