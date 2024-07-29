using MQContract.Interfaces.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Messages
{
    public record ServiceQueryResult(string ID, IMessageHeader Header,string MessageTypeID,ReadOnlyMemory<byte> Data)
        : IEncodedMessage
    {
    }
}
