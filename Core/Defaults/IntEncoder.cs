using MQContract.Interfaces.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Defaults
{
    internal class IntEncoder : IMessageTypeEncoder<int>
    {
        async ValueTask<int> IMessageTypeEncoder<int>.DecodeAsync(Stream stream)
        => BitConverter.ToInt32(await BitConverterHelper.StreamToByteArray(stream));

        ValueTask<byte[]> IMessageTypeEncoder<int>.EncodeAsync(int message)
        => ValueTask.FromResult(BitConverter.GetBytes(message));
    }
}
