using MQContract.ServiceAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedTesting
{
    internal record TestServiceChannelOptions(string TestName):IServiceChannelOptions{}
}
