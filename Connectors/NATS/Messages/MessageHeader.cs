using MQContract.Messages;
using NATS.Client.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.NATS.Messages
{
    internal class MessageHeader(NatsHeaders? headers) : IMessageHeader
    {
        public string? this[string tagKey] 
            => (headers?.TryGetValue(tagKey,out var value)??false ? value.ToString() : null);

        public IEnumerable<string> Keys 
            => headers?.Keys?? [];
    }
}
