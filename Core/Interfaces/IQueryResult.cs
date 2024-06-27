using MQContract.ServiceAbstractions.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Interfaces
{
    public interface IQueryResult<out T> : ITransmissionResult where T : class
    {
        T? Result { get; }
        IMessageHeader Header { get; }
    }
}
