using MQContract.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedTesting.Messages
{
    [MessageChannel("NamedAndVersioned")]
    [MessageName("VersionedMessage")]
    [MessageVersion("1.0.0.3")]
    public record NamedAndVersionedMessage(string TestName){}
}
