using MQContract.Interfaces.Encrypting;
using MQContract.Messages;

namespace MQContract.Defaults
{
    internal class NonEncryptor<T> : IMessageTypeEncryptor<T>
    {
        public ValueTask<Stream> DecryptAsync(Stream stream, MessageHeader headers) 
            => ValueTask.FromResult(stream);

        public ValueTask<byte[]> EncryptAsync(byte[] data, out Dictionary<string, string?> headers)
        {
            headers = [];
            return ValueTask.FromResult(data);
        }
    }
}
