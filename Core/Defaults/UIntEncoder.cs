using MQContract.Interfaces.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Defaults
{
    internal class UIntEncoder : IMessageTypeEncoder<uint>
    {
        async ValueTask<uint> IMessageTypeEncoder<uint>.DecodeAsync(Stream stream)
        => BitConverter.ToUInt32(await BitConverterHelper.StreamToByteArray(stream));

        ValueTask<byte[]> IMessageTypeEncoder<uint>.EncodeAsync(uint message)
        => ValueTask.FromResult(BitConverter.GetBytes(message));
    }
}
