using CodeGenTesting.Messages;
using MQContract.Interfaces.Encrypting;
using MQContract.Messages;

namespace CodeGenTesting.Encryptors;

internal class NonContextMessageEncryptor : IMessageTypeEncryptor<NonContextMessage>
{
    ValueTask<Stream> IMessageEncryptor.DecryptAsync(Stream stream, MessageHeader headers)
        => throw new NotImplementedException();

    ValueTask<EncryptionResult> IMessageEncryptor.EncryptAsync(byte[] data)
        => throw new NotImplementedException();
}
