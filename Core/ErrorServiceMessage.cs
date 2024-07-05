using MQContract.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract
{
    internal record ErrorServiceMessage :
        ServiceMessage,IErrorServiceMessage
    {
        public Exception Error { get; private init; }
        public ErrorServiceMessage(Guid id, string messageTypeID, string channel, IMessageHeader header, Exception error)
            : base(id, messageTypeID, channel, header, [])
        {
            Error = error;
        }
    }
}
