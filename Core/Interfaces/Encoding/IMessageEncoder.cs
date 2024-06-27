namespace MQContract.Interfaces.Encoding
{
    /// <summary>
    /// An implementation of this is used to encode/decode message bodies when 
    /// specified for a connection.  This is to allow for an override of the 
    /// default encoding of Json for the messages.
    /// </summary>
    public interface IMessageEncoder
    {
        /// <summary>
        /// Called to encode a message into a byte array
        /// </summary>
        /// <typeparam name="T">The type of message being encoded</typeparam>
        /// <param name="message">The message being encoded</param>
        /// <returns>A byte array of the message in it's encoded form that will be transmitted</returns>
        byte[] Encode<T>(T message);

        /// <summary>
        /// Called to decode a message from a byte array
        /// </summary>
        /// <typeparam name="T">The type of message being decoded</typeparam>
        /// <param name="stream">A stream representing the byte array data that was transmitted as the message body in KubeMQ</param>
        /// <returns>Null when fails or the value of T that was encoded inside the stream</returns>
        T? Decode<T>(Stream stream);
    }
}
