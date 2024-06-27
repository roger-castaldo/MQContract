using MQContract.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedTesting.Messages
{
    [MessageChannel("CustomEncoderWithInjection")]
    public record CustomEncoderWithInjectionMessage(string TestName){}
}
