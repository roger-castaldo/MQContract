using AutomatedTesting.Messages;
using MQContract.Interfaces.Encoding;
using System.Text;

namespace AutomatedTesting.Encoders
{
    internal class TestMessageEncoder : IMessageTypeEncoder<CustomEncoderMessage>
    {
        public CustomEncoderMessage? Decode(Stream stream)
            => new CustomEncoderMessage(Encoding.ASCII.GetString(new BinaryReader(stream).ReadBytes((int)stream.Length)));

        public byte[] Encode(CustomEncoderMessage message)
            => Encoding.ASCII.GetBytes(message.TestName);
    }
}
