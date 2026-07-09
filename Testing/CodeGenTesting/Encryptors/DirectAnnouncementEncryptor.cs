using CodeGenTesting.Messages;
using Microsoft.Extensions.DependencyInjection;
using MQContract.Interfaces.Encrypting;
using MQContract.Messages;

namespace CodeGenTesting.Encryptors;

internal class DirectAnnouncementEncryptor : IMessageTypeEncryptor<DirectAnnouncement>
{
    public DirectAnnouncementEncryptor() { }

    [ActivatorUtilitiesConstructor]
    public DirectAnnouncementEncryptor(IServiceInjection serviceInjection)
    {
        ArgumentNullException.ThrowIfNull(serviceInjection, nameof(serviceInjection));
    }

    ValueTask<Stream> IMessageEncryptor.DecryptAsync(Stream stream, MessageHeader headers)
        => ValueTask.FromResult<Stream>(stream);

    ValueTask<EncryptionResult> IMessageEncryptor.EncryptAsync(byte[] data)
        => ValueTask.FromResult<EncryptionResult>(new(null, data));
}
