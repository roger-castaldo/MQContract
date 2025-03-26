using MQContract.Interfaces.Encrypting;
using MQContract.Messages;

namespace MQContract.Defaults
{
    internal class NonEncryptor<T> : IMessageTypeEncryptor<T>
    {
        public ValueTask<Stream> DecryptAsync(Stream stream, MessageHeader headers)
            => ValueTask.FromResult(stream);

        public ValueTask<(byte[] data, Dictionary<string, string?> headers)> EncryptAsync(byte[] data)
            => ValueTask.FromResult<(byte[] data, Dictionary<string, string?> headers)>((data, []));
    }
}
