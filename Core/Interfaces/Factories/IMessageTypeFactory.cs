using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Interfaces.Factories
{
    internal interface IMessageTypeFactory
    {
        bool IgnoreMessageHeader { get; }
    }
}
