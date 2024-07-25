using MQContract.Messages;
using NATS.Client.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.NATS.Messages
{
    internal class PingResponse(TimeSpan responseTime, INatsServerInfo? serverInfo)
        : IPingResult
    {
        public string Host => serverInfo?.Host??string.Empty;

        public string Version => serverInfo?.Version??string.Empty;

        public TimeSpan ResponseTime => responseTime;
    }
}
