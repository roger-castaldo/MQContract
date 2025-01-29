using MQContract.Interfaces.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Defaults
{
    internal class HalfEncoder : IMessageTypeEncoder<Half>
    {
        async ValueTask<Half> IMessageTypeEncoder<Half>.DecodeAsync(Stream stream)
        => BitConverter.ToHalf(await BitConverterHelper.StreamToByteArray(stream));

        ValueTask<byte[]> IMessageTypeEncoder<Half>.EncodeAsync(Half message)
        => ValueTask.FromResult(BitConverter.GetBytes(message));
    }
}
