using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.NATS.Messages
{
    [ProtoContract]
    internal record NatsQueryResponseMessage
    {
        [ProtoMember(1)]
        public string ID { get; set; } = string.Empty;
        [ProtoMember(2)]
        public string MessageTypeID { get; set; } = string.Empty;
        [ProtoMember(3)]
        public ReadOnlyMemory<byte>? Data { get; set; } = null;
        [ProtoMember(4)]
        public string? Error { get; set; } = null;
    }
}
