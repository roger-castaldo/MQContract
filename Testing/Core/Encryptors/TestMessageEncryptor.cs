using CoreTesting.Messages;
using MQContract.Interfaces.Encrypting;

namespace CoreTesting.Encryptors
{
    internal class TestMessageEncryptor : IMessageTypeEncryptor<CustomEncryptorMessage>
    {
        private const string HeaderKey = "TestMessageEncryptorKey";
        private const string HeaderValue = "TestMessageEncryptorValue";

        public ValueTask<Stream> DecryptAsync(Stream stream, MessageHeader headers)
        {
            Assert.IsNotNull(headers);
            Assert.Contains(HeaderKey, headers.Keys);
            Assert.AreEqual(HeaderValue, headers[HeaderKey]);
            var data = new BinaryReader(stream).ReadBytes((int)stream.Length);
            return ValueTask.FromResult<Stream>(new MemoryStream(data.Reverse().ToArray()));
        }

        public ValueTask<EncryptionResult> EncryptAsync(byte[] data)
            => ValueTask.FromResult<EncryptionResult>(new(
                new([new(HeaderKey, HeaderValue)]),
                [.. data.Reverse()]
            ));
    }
}
