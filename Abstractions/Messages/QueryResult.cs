using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Messages
{
    public record QueryResult<T>(string ID,IMessageHeader Header,T? Result=null,string? Error=null)
        : TransmissionResult(ID,Error)
        where T : class
    {}
}
