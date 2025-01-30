using MQContract.Interfaces.Encoding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Defaults
{
    internal class BooleanEncoder : IMessageTypeEncoder<bool>
    {
        async ValueTask<bool> IMessageTypeEncoder<bool>.DecodeAsync(Stream stream)
            => BitConverter.ToBoolean(await BitConverterHelper.StreamToByteArray(stream));

        ValueTask<byte[]> IMessageTypeEncoder<bool>.EncodeAsync(bool message)
            => ValueTask.FromResult(BitConverter.GetBytes(message));
    }
}
