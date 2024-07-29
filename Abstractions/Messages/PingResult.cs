using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Messages
{
    public record PingResult(string Host,string Version, TimeSpan ResponseTime)
    {}
}
