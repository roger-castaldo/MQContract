using MQContract.Interfaces;
using MQContract.ServiceAbstractions.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Messages
{
    internal record QueryResult<T>(T? Result,IMessageHeader Header,string MessageID,bool IsError,string? Error)
        : IQueryResult<T> where T:class;
}
