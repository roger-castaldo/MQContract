﻿using MQContract.Messages;

namespace MQContract.Interfaces.Middleware
{
    /// <summary>
    /// This interface represents a Middleware to execute Before a specific message type is encoded
    /// </summary>
    public interface IBeforeEncodeSpecificTypeMiddleware<T> : ISpecificTypeMiddleware<T>
        where T : class
    {
        /// <summary>
        /// This is the method invoked as part of the Middle Ware processing during message encoding
        /// </summary>
        /// <param name="context">A shared context that exists from the start of this encoding instance</param>
        /// <param name="message">The message being encoded</param>
        /// <param name="channel">The channel this message was requested to transmit to</param>
        /// <param name="messageHeader">The message headers being supplied</param>
        /// <returns>The message, channel and header to allow for changes if desired</returns>
        ValueTask<(T message,string? channel,MessageHeader messageHeader)> BeforeMessageEncodeAsync(IContext context,T message, string? channel,MessageHeader messageHeader);
    }
}
