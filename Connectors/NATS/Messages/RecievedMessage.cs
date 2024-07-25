using MQContract.Messages;
using NATS.Client.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.NATS.Messages
{
    internal class RecievedMessage(MessageHeader header,string Subject,NatsMessage? msg) : IRecievedServiceMessage
    {
        public DateTime RecievedTimestamp { get; private init; } = DateTime.Now;

        public string MessageTypeID => msg?.MessageTypeID??string.Empty;

        public string Channel => Subject;

        public IMessageHeader Header => header;

        public ReadOnlyMemory<byte> Data => msg?.Data??new ReadOnlyMemory<byte>();

        public string ID => msg?.ID??string.Empty;
    }
}
