using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Messages
{
    public interface IErrorServiceMessage : IServiceMessage
    {
        Exception Error { get; }
    }
}
