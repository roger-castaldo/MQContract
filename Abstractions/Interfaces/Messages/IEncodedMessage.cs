using MQContract.Messages;

namespace MQContract.Interfaces.Messages
{
    /// <summary>
    /// Used to house an underlying message that has been encoded and is ready to be "shipped" into the underlying service layer
    /// </summary>
    public interface IEncodedMessage
    {
        /// <summary>
        /// The header for the given message
        /// </summary>
        MessageHeader Header { get; }
        /// <summary>
        /// The message type id to transmit across
        /// </summary>
        string MessageTypeID { get; }
        /// <summary>
        /// The encoded message
        /// </summary>
        ReadOnlyMemory<byte> Data { get; }
    }
}
