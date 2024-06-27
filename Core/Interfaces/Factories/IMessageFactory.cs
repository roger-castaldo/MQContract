using MQContract.Interfaces.Conversion;
using MQContract.ServiceAbstractions.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Interfaces.Factories
{
    internal interface IMessageFactory<T> : IMessageTypeFactory, IConversionPath<T> where T : class
    {
        IServiceMessage ConvertMessage(T message, string? channel, IMessageHeader? messageHeader = null);
    }
}
