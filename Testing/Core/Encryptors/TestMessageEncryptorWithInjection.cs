using CoreTesting.Messages;
using CoreTesting.ServiceInjection;
using MQContract.Interfaces.Encrypting;

namespace CoreTesting.Encryptors
{
    internal class TestMessageEncryptorWithInjection(IInjectableService injectableService)
        : IMessageTypeEncryptor<CustomEncryptorWithInjectionMessage>
    {
        private const string HeaderKey = "TestMessageEncryptorWithInjectionKey";

        public ValueTask<Stream> DecryptAsync(Stream stream, MessageHeader headers)
        {
            Assert.IsNotNull(headers);
            Assert.Contains(HeaderKey, headers.Keys);
            Assert.AreEqual(injectableService.Name, headers[HeaderKey]);
            var data = new BinaryReader(stream).ReadBytes((int)stream.Length);
            return ValueTask.FromResult<Stream>(new MemoryStream(data.Reverse().ToArray()));
        }

        public ValueTask<EncryptionResult> EncryptAsync(byte[] data)
            => ValueTask.FromResult<EncryptionResult>(new(
                new Dictionary<string, string?>([
                    new(HeaderKey,injectableService.Name)
                ]),
                [.. data.Reverse()]
            ));
    }
}
