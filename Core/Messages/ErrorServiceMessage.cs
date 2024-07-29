using MQContract.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Channels;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MQContract.Messages
{
    internal static class ErrorServiceMessage
    {
        public const string MessageTypeID = "U-InternalServiceErrorMessage-1.0.0";

        public static ServiceMessage Produce(string channel,Exception error)
            =>new(Guid.NewGuid().ToString(), MessageTypeID, channel, new MessageHeader([]), EncodeError(error));

        private static byte[] EncodeError(Exception error)
            => UTF8Encoding.UTF8.GetBytes(error.Message);

        public static QueryResponseException DecodeError(ReadOnlyMemory<byte> data)
            => new(UTF8Encoding.UTF8.GetString(data.ToArray()));
    }
}
