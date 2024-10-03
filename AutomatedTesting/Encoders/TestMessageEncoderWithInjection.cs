using AutomatedTesting.Messages;
using AutomatedTesting.ServiceInjection;
using MQContract.Interfaces.Encoding;
using System.Text;

namespace AutomatedTesting.Encoders
{
    internal class TestMessageEncoderWithInjection(IInjectableService injectableService)
        : IMessageTypeEncoder<CustomEncoderWithInjectionMessage>
    {
        public ValueTask<CustomEncoderWithInjectionMessage?> DecodeAsync(Stream stream)
        {
            var message = Encoding.ASCII.GetString(new BinaryReader(stream).ReadBytes((int)stream.Length));
            Assert.IsTrue(message.StartsWith($"{injectableService.Name}:"));
            return ValueTask.FromResult<CustomEncoderWithInjectionMessage?>(new CustomEncoderWithInjectionMessage(message.Substring($"{injectableService.Name}:".Length)));
        }

        public ValueTask<byte[]> EncodeAsync(CustomEncoderWithInjectionMessage message)
            => ValueTask.FromResult<byte[]>(Encoding.ASCII.GetBytes($"{injectableService.Name}:{message.TestName}"));
    }
}
