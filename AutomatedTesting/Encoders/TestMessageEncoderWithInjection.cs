using AutomatedTesting.Messages;
using AutomatedTesting.ServiceInjection;
using MQContract.Interfaces.Encoding;
using System.Text;

namespace AutomatedTesting.Encoders
{
    internal class TestMessageEncoderWithInjection(IInjectableService injectableService)
        : IMessageTypeEncoder<CustomEncoderWithInjectionMessage>
    {
        public CustomEncoderWithInjectionMessage? Decode(Stream stream)
        {
            var message = Encoding.ASCII.GetString(new BinaryReader(stream).ReadBytes((int)stream.Length));
            Assert.IsTrue(message.StartsWith($"{injectableService.Name}:"));
            return new CustomEncoderWithInjectionMessage(message.Substring($"{injectableService.Name}:".Length));
        }

        public byte[] Encode(CustomEncoderWithInjectionMessage message)
            => Encoding.ASCII.GetBytes($"{injectableService.Name}:{message.TestName}");
    }
}
