using MQContract.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Connections
{
    internal readonly record struct FilteredServiceMessage(ServiceMessage? ServiceMessage, Activity? Activity, MessageFilterResult FilterResult);
}
