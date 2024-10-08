﻿using MQContract.Messages;

namespace MQContract.Interfaces.Middleware
{
    /// <summary>
    /// This interface represents a Middleware to execute after a Message has been decoded from a ServiceMessage to the expected Class
    /// </summary>
    public interface IAfterDecodeMiddleware : IMiddleware
    {
        /// <summary>
        /// This is the method invoked as part of the Middleware processing during message decoding
        /// </summary>
        /// <typeparam name="T">This will be the type of the Message that was decoded</typeparam>
        /// <param name="context">A shared context that exists from the start of this decode process instance</param>
        /// <param name="message">The class message</param>
        /// <param name="ID">The id of the message</param>
        /// <param name="messageHeader">The headers from the message</param>
        /// <param name="receivedTimestamp">The timestamp of when the message was recieved</param>
        /// <param name="processedTimeStamp">The timestamp of when the message was decoded into a Class</param>
        /// <returns>The message and header to allow for changes if desired</returns>
        ValueTask<(T message,MessageHeader messageHeader)> AfterMessageDecodeAsync<T>(IContext context, T message, string ID,MessageHeader messageHeader,DateTime receivedTimestamp,DateTime processedTimeStamp);
    }
}
