using MQContract.Messages;
using System.Diagnostics;

namespace MQContract.Interfaces
{
    /// <summary>
    /// An interface for describing a Message received on a Subscription to be passed into the appropriate callback
    /// </summary>
    /// <typeparam name="TMessage">The class type of the underlying message</typeparam>
    public interface IReceivedMessage<out TMessage>
    {
        /// <summary>
        /// The unique ID of the received message that was specified on the transmission side
        /// </summary>
        string ID { get; }
        /// <summary>
        /// The message that was transmitted
        /// </summary>
        TMessage Message { get; }
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
        /// <summary>
        /// The Activity, used for OTel associated with this recieved message
        /// </summary>
        Activity? Activity { get; }
    }
}
