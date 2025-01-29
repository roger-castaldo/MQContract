using MQContract.Interfaces.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Defaults
{
    internal class ShortEncoder : IMessageTypeEncoder<short>
    {
        async ValueTask<short> IMessageTypeEncoder<short>.DecodeAsync(Stream stream)
        => BitConverter.ToInt16(await BitConverterHelper.StreamToByteArray(stream));

        ValueTask<byte[]> IMessageTypeEncoder<short>.EncodeAsync(short message)
        => ValueTask.FromResult(BitConverter.GetBytes(message));
    }
}
