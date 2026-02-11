using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Loggers
{
    /// base log ids 1000-1999
    internal static partial class BaseLog
    {
        [LoggerMessage(
            EventId = 1100,
            Level = LogLevel.Information,
            Message = "Calling subscription {ID} end async"
            )]
        public static partial void SubscriptionEndAsync(ILogger? logger, Guid id);
    }
}
