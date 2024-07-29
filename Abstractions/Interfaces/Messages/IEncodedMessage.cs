using MQContract.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Interfaces.Messages
{
    public interface IEncodedMessage
    {
        IMessageHeader Header { get; }
        string MessageTypeID { get; }
        ReadOnlyMemory<byte> Data { get; }
    }
}
