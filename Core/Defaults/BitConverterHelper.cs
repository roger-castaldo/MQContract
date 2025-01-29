using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Defaults
{
    internal static class BitConverterHelper
    {
        public static async ValueTask<byte[]> StreamToByteArray(Stream stream)
        {
            using var bufferedStream = new BufferedStream(stream);
            using var memoryStream = new MemoryStream();
            await bufferedStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
