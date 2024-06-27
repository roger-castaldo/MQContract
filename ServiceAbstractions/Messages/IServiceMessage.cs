using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.ServiceAbstractions.Messages
{
    public interface IServiceMessage
    {
        string ID { get; }
        string MessageTypeID { get; }
        string Channel { get; }
        IMessageHeader Header { get; }
        ReadOnlyMemory<byte> Data { get; }
    }
}
