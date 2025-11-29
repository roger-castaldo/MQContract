using MQContract.Messages;

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
        /// Called to decrypt the message body stream received as a message
        /// </summary>
        /// <param name="stream">The stream representing the message body binary data</param>
        /// <param name="headers">The message headers that were provided by the message</param>
        /// <returns>A decrypted stream of the message body</returns>
        ValueTask<Stream> DecryptAsync(Stream stream, MessageHeader headers);

        /// <summary>
        /// Called to encrypt the message body prior to transmitting a message
        /// </summary>
        /// <param name="data">The original unencrypted body data</param>
        /// <returns>An encrypted byte array of the message body and any headers that might be needed</returns>
        ValueTask<EncryptionResult> EncryptAsync(byte[] data);
    }
}
