using CodeGenTesting.Messages;
using MQContract.Interfaces.Encrypting;
using MQContract.Messages;

namespace CodeGenTesting.Encryptors
{
    internal class DirectAnnouncementEncryptor : IMessageTypeEncryptor<DirectAnnouncement>
    {
        ValueTask<Stream> IMessageEncryptor.DecryptAsync(Stream stream, MessageHeader headers)
            => ValueTask.FromResult<Stream>(stream);

        ValueTask<EncryptionResult> IMessageEncryptor.EncryptAsync(byte[] data)
            => ValueTask.FromResult<EncryptionResult>(new(null, data));
    }
}
