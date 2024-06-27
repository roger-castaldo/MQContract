using MQContract.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedTesting.Messages
{
    [MessageChannel("CustomEncoder")]
    public record CustomEncoderMessage(string TestName){}
}
