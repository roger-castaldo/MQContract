using MQContract.Interfaces.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Defaults
{
    internal class DecimalEncoder : IMessageTypeEncoder<decimal>
    {
        private const int BitsPerDecimal = 4;

        async ValueTask<decimal> IMessageTypeEncoder<decimal>.DecodeAsync(Stream stream)
        {
            var byteData = await BitConverterHelper.StreamToByteArray(stream);

            var bits = new int[BitsPerDecimal];
            for (var i = 0; i<bits.Length; i++)
                bits[i] = BitConverter.ToInt32(byteData, i*sizeof(int));

            return new decimal(bits);
        }

        ValueTask<byte[]> IMessageTypeEncoder<decimal>.EncodeAsync(decimal message)
        {
            var result = new byte[sizeof(int)*BitsPerDecimal];

            var bits = decimal.GetBits(message);

            for (var i = 0; i<bits.Length; i++)
                Buffer.BlockCopy(BitConverter.GetBytes(bits[i]), 0, result, i*sizeof(int), sizeof(int));

            return ValueTask.FromResult(result);
        }
    }
}
