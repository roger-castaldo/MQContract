using AutomatedTesting.Messages;
using MQContract.Interfaces.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
