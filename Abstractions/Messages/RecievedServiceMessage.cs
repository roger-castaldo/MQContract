using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Messages
{
    public record RecievedServiceMessage(string ID, string MessageTypeID, string Channel, IMessageHeader Header, ReadOnlyMemory<byte> Data)
        : ServiceMessage(ID,MessageTypeID,Channel,Header,Data)
    { 
        public DateTime RecievedTimestamp { get; private init; } = DateTime.Now;
    }
}
