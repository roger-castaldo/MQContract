using MQContract.Interfaces.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Defaults
{
    internal class ByteEncoder : IMessageTypeEncoder<byte>
    {
        ValueTask<byte> IMessageTypeEncoder<byte>.DecodeAsync(Stream stream)
            => ValueTask.FromResult((byte)stream.ReadByte());

        ValueTask<byte[]> IMessageTypeEncoder<byte>.EncodeAsync(byte message)
            => ValueTask.FromResult<byte[]>([message]);
    }
}
