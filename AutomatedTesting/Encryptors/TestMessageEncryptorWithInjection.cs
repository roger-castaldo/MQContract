using AutomatedTesting.Messages;
using AutomatedTesting.ServiceInjection;
using MQContract.Interfaces.Encrypting;

namespace AutomatedTesting.Encryptors
{
    internal class TestMessageEncryptorWithInjection(IInjectableService injectableService)
        : IMessageTypeEncryptor<CustomEncryptorWithInjectionMessage>
    {
        private const string HeaderKey = "TestMessageEncryptorWithInjectionKey";

        public ValueTask<Stream> DecryptAsync(Stream stream, MessageHeader headers)
        {
            Assert.IsNotNull(headers);
            Assert.IsTrue(headers.Keys.Contains(HeaderKey));
            Assert.AreEqual(injectableService.Name, headers[HeaderKey]);
            var data = new BinaryReader(stream).ReadBytes((int)stream.Length);
            return ValueTask.FromResult<Stream>(new MemoryStream(data.Reverse().ToArray()));
        }

        public ValueTask<byte[]> EncryptAsync(byte[] data, out Dictionary<string, string?> headers)
        {
            headers = new([
                new(HeaderKey,injectableService.Name)
                ]);
            return ValueTask.FromResult<byte[]>(data.Reverse().ToArray());
        }
    }
}
