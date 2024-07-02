using MQContract.Interfaces.Encrypting;
using MQContract.ServiceAbstractions.Messages;

namespace MQContract.Defaults
{
    internal class NonEncryptor<T> : IMessageTypeEncryptor<T>
    {
        public Stream Decrypt(Stream stream, IMessageHeader headers) => stream;

        public byte[] Encrypt(byte[] data, out Dictionary<string, string?> headers)
        {
            headers = [];
            return data;
        }
    }
}
