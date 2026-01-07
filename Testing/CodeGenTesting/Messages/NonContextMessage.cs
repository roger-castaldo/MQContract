using MQContract.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenTesting.Messages
{
    [Message()]
    internal record NonContextMessage(string Message)
    {}
}
