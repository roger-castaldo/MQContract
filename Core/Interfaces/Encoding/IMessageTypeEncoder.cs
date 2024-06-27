namespace MQContract.Interfaces.Encoding
{
    /// <summary>
    /// Used to define a specific encoder for the message type of T
    /// This is used to override the default Json and the Global one for the connection if specified
    /// </summary>
    /// <typeparam name="T">The type of message that this encoder supports</typeparam>
    public interface IMessageTypeEncoder<T>
    {
        /// <summary>
        /// Called to encode the message into a byte array
        /// </summary>
        /// <param name="message">The message value to encode</param>
        /// <returns>The message encoded as a byte array</returns>
        byte[] Encode(T message);
        /// <summary>
        /// Called to decode the message from a byte stream into the specified type
        /// </summary>
        /// <param name="stream">The byte stream containing the encoded message</param>
        /// <returns>null if the Decode fails, otherwise an instance of the message decoded from the stream</returns>
        T? Decode(Stream stream);
    }
}
