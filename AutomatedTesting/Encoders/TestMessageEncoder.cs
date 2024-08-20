using AutomatedTesting.Messages;
using MQContract.Interfaces.Encoding;
using System.Text;

namespace AutomatedTesting.Encoders
{
    internal class TestMessageEncoder : IMessageTypeEncoder<CustomEncoderMessage>
    {
        public ValueTask<CustomEncoderMessage?> DecodeAsync(Stream stream)
            => ValueTask.FromResult<CustomEncoderMessage?>(new CustomEncoderMessage(Encoding.ASCII.GetString(new BinaryReader(stream).ReadBytes((int)stream.Length))));

        public ValueTask<byte[]> EncodeAsync(CustomEncoderMessage message)
            => ValueTask.FromResult<byte[]>(Encoding.ASCII.GetBytes(message.TestName));
    }
}
