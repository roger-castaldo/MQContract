using AutomatedTesting.Messages;
using AutomatedTesting.ServiceInjection;
using MQContract.Interfaces.Encrypting;
using MQContract.ServiceAbstractions.Messages;

namespace AutomatedTesting.Encryptors
{
    internal class TestMessageEncryptorWithInjection(IInjectableService injectableService)
        : IMessageTypeEncryptor<CustomEncryptorWithInjectionMessage>
    {
        private const string HeaderKey = "TestMessageEncryptorWithInjectionKey";

        public Stream Decrypt(Stream stream, IMessageHeader headers)
        {
            Assert.IsNotNull(headers);
            Assert.IsTrue(headers.Keys.Contains(HeaderKey));
            Assert.AreEqual(injectableService.Name, headers[HeaderKey]);
            var data = new BinaryReader(stream).ReadBytes((int)stream.Length);
            return new MemoryStream(data.Reverse().ToArray());
        }

        public byte[] Encrypt(byte[] data, out Dictionary<string, string?> headers)
        {
            headers = new([
                new(HeaderKey,injectableService.Name)
                ]);
            return data.Reverse().ToArray();
        }
    }
}
