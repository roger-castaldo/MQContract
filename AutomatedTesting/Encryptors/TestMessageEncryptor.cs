using AutomatedTesting.Messages;
using MQContract.Interfaces.Encrypting;
using MQContract.Messages;

namespace AutomatedTesting.Encryptors
{
    internal class TestMessageEncryptor : IMessageTypeEncryptor<CustomEncryptorMessage>
    {
        private const string HeaderKey = "TestMessageEncryptorKey";
        private const string HeaderValue = "TestMessageEncryptorValue";

        public Stream Decrypt(Stream stream, IMessageHeader headers)
        {
            Assert.IsNotNull(headers);
            Assert.IsTrue(headers.Keys.Contains(HeaderKey));
            Assert.AreEqual(HeaderValue, headers[HeaderKey]);
            var data = new BinaryReader(stream).ReadBytes((int)stream.Length);
            return new MemoryStream(data.Reverse().ToArray());
        }

        public byte[] Encrypt(byte[] data, out Dictionary<string, string?> headers)
        {
            headers = new([
                new(HeaderKey,HeaderValue)
                ]);
            return data.Reverse().ToArray();
        }
    }
}
