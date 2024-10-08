﻿using MQContract.Messages;

namespace MQContract.Interfaces
{
    /// <summary>
    /// An interface for describing a Message received on a Subscription to be passed into the appropriate callback
    /// </summary>
    /// <typeparam name="T">The class type of the underlying message</typeparam>
    public interface IReceivedMessage<out T>
        where T : class
    {
        /// <summary>
        /// The unique ID of the received message that was specified on the transmission side
        /// </summary>
        string ID { get; }
        /// <summary>
        /// The message that was transmitted
        /// </summary>
        T Message { get; }
        /// <summary>
        /// The headers that were supplied with the message
        /// </summary>
        MessageHeader Headers { get; }
        /// <summary>
        /// The timestamp of when the message was received by the underlying service connection
        /// </summary>
        DateTime ReceivedTimestamp { get; }
        /// <summary>
        /// The timestamp of when the received message was converted into the actual class prior to calling the callback
        /// </summary>
        DateTime ProcessedTimestamp { get; }
    }
}
