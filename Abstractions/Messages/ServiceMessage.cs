using MQContract.Interfaces.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Messages
{
    public record ServiceMessage(string ID,string MessageTypeID,string Channel,IMessageHeader Header,ReadOnlyMemory<byte> Data)
        : IEncodedMessage
    { }
}
