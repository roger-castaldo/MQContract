using MQContract.ServiceAbstractions.Messages;

namespace MQContract.Interfaces.Encrypting
{
    /// <summary>
    /// An implementation of this is used to encrypt/decrypt message bodies when 
    /// specified for a connection.  This is to allow for extended message security
    /// if desired.
    /// </summary>
    public interface IMessageEncryptor
    {
        /// <summary>
        /// Called to decrypt the message body stream recieved as a message
        /// </summary>
        /// <param name="stream">The stream representing the message body binary data</param>
        /// <param name="headers">The message headers that were provided by the message</param>
        /// <returns>A decrypted stream of the message body</returns>
        Stream Decrypt(Stream stream, IMessageHeader headers);

        /// <summary>
        /// Called to encrypt the message body prior to transmitting a message
        /// </summary>
        /// <param name="data">The original unencrypted body data</param>
        /// <param name="headers">The headers that are desired to attache to the message if needed</param>
        /// <returns>An encrypted byte array of the message body</returns>
        byte[] Encrypt(byte[] data, out Dictionary<string, string?> headers);
    }
}
