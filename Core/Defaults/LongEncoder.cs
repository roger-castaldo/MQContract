using MQContract.Interfaces.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Defaults
{
    internal class ULongEncoder : IMessageTypeEncoder<ulong>
    {
        async ValueTask<ulong> IMessageTypeEncoder<ulong>.DecodeAsync(Stream stream)
        => BitConverter.ToUInt64(await BitConverterHelper.StreamToByteArray(stream));

        ValueTask<byte[]> IMessageTypeEncoder<ulong>.EncodeAsync(ulong message)
        => ValueTask.FromResult(BitConverter.GetBytes(message));
    }
}
