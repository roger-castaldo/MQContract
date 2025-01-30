using MQContract.Interfaces.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Defaults
{
    internal class UShortEncoder : IMessageTypeEncoder<ushort>
    {
        async ValueTask<ushort> IMessageTypeEncoder<ushort>.DecodeAsync(Stream stream)
        => BitConverter.ToUInt16(await BitConverterHelper.StreamToByteArray(stream));

        ValueTask<byte[]> IMessageTypeEncoder<ushort>.EncodeAsync(ushort message)
        => ValueTask.FromResult(BitConverter.GetBytes(message));
    }
}
