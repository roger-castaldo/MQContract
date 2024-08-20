using AutomatedTesting.Messages;
using MQContract.Interfaces.Encrypting;

namespace AutomatedTesting.Encryptors
{
    internal class TestMessageEncryptor : IMessageTypeEncryptor<CustomEncryptorMessage>
    {
        private const string HeaderKey = "TestMessageEncryptorKey";
        private const string HeaderValue = "TestMessageEncryptorValue";

        public ValueTask<Stream> DecryptAsync(Stream stream, MessageHeader headers)
        {
            Assert.IsNotNull(headers);
            Assert.IsTrue(headers.Keys.Contains(HeaderKey));
            Assert.AreEqual(HeaderValue, headers[HeaderKey]);
            var data = new BinaryReader(stream).ReadBytes((int)stream.Length);
            return ValueTask.FromResult<Stream>(new MemoryStream(data.Reverse().ToArray()));
        }

        public ValueTask<byte[]> EncryptAsync(byte[] data, out Dictionary<string, string?> headers)
        {
            headers = new([
                new(HeaderKey,HeaderValue)
                ]);
            return ValueTask.FromResult<byte[]>(data.Reverse().ToArray());
        }
    }
}
