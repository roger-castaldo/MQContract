using MQContract.Messages;
using System.Diagnostics;

namespace MQContract.Connections
{
    internal readonly record struct FilteredServiceMessage(ServiceMessage? ServiceMessage, Activity? Activity, MessageFilterResult FilterResult);
}
