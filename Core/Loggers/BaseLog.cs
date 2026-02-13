using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace MQContract.Loggers
{
    /// base log ids 1000-1999
    /// 1000-1099 - connection related
    /// 1100-1199 - subscription related
    /// 1200-1299 - resilience related
    /// 1300-1399 - middleware related
    /// 1400-1499 - service message related
    /// 1900-1999 - error related
    internal static partial class BaseLog
    {
        [LoggerMessage(
           EventId = 1000,
           Level = LogLevel.Debug,
           Message = "Closing contract connection"
           )]
        public static partial void ClosingConnection(ILogger logger);

        [LoggerMessage(
           EventId = 1001,
           Level = LogLevel.Information,
           Message = "Closing all open inbox subscriptions"
           )]
        public static partial void ClosingAllInboxes(ILogger logger);

        [LoggerMessage(
           EventId = 1002,
           Level = LogLevel.Debug,
           Message = "Attempting to call Ping against an underlying service connection"
           )]
        public static partial void PingServiceConnection(ILogger logger);

        [LoggerMessage(
            EventId = 1100,
            Level = LogLevel.Information,
            Message = "Calling subscription {ID} end async"
            )]
        public static partial void SubscriptionEndAsync(ILogger logger, Guid id);

        [LoggerMessage(
            EventId = 1200,
            Level = LogLevel.Debug,
            Message = "Retry fallback has been triggered for {MessageID}"
            )]
        public static partial void ResilienceRetryTriggered(ILogger logger, string messageID);

        [LoggerMessage(
            EventId = 1300,
            Level = LogLevel.Debug,
            Message = "Registering middleware of type {Type}"
            )]
        public static partial void RegisteringMiddleware(ILogger logger, Type type);

        [LoggerMessage(
            EventId = 1301,
            Level = LogLevel.Debug,
            Message = "Executing generic Before Message Encode middleware for message of type {Type}"
            )]
        public static partial void ExecutingGenericBeforeEncodeMiddleware(ILogger logger, Type type);

        [LoggerMessage(
            EventId = 1302,
            Level = LogLevel.Debug,
            Message = "Executing specific for type Before Message Encode middleware for message of type {Type}"
            )]
        public static partial void ExecutingSpecificBeforeEncodeMiddleware(ILogger logger, Type type);

        [LoggerMessage(
            EventId = 1303,
            Level = LogLevel.Debug,
            Message = "Executing generic After Message Encode middleware for message of type {Type}"
            )]
        public static partial void ExecutingGenericAfterEncodeMiddleware(ILogger logger, Type type);

        [LoggerMessage(
            EventId = 1304,
            Level = LogLevel.Debug,
            Message = "Executing generic Before Message Decode middleware"
            )]
        public static partial void ExecutingGenericBeforeDecodeMiddleware(ILogger logger);

        [LoggerMessage(
            EventId = 1305,
            Level = LogLevel.Debug,
            Message = "Executing generic After Message Decode middleware for message of type {Type}"
            )]
        public static partial void ExecutingGenericAfterDecodeMiddleware(ILogger logger, Type type);

        [LoggerMessage(
            EventId = 1306,
            Level = LogLevel.Debug,
            Message = "Executing specific for type After Message Decode middleware for message of type {Type}"
            )]
        public static partial void ExecutingSpecificAfterDecodeMiddleware(ILogger logger, Type type);

        [LoggerMessage(
            EventId = 1307,
            Level = LogLevel.Debug,
            Message = "Enabling metrics on service connection with {Meter} and {UseInternal}"
            )]
        public static partial void EnablingMetricMiddleware(ILogger logger, Meter? meter, bool useInternal);

        [LoggerMessage(
            EventId = 1400,
            Level = LogLevel.Debug,
            Message = "Producing Service Message for message of type {Type}"
            )]
        public static partial void ProducingServiceMessage(ILogger logger, Type type);

        [LoggerMessage(
            EventId = 1401,
            Level = LogLevel.Debug,
            Message = "Filtering Service Message message of type {Type} by headers"
            )]
        public static partial void FilteringServiceMessageByHeaders(ILogger logger, Type type);

        [LoggerMessage(
            EventId = 1402,
            Level = LogLevel.Debug,
            Message = "Decoding Service Message message of type {Type}"
            )]
        public static partial void DecodingServiceMessage(ILogger logger, Type type);

        [LoggerMessage(
            EventId = 1403,
            Level = LogLevel.Debug,
            Message = "Filtering Service Message message of type {Type} in full"
            )]
        public static partial void FilteringServiceMessageInFull(ILogger logger, Type type);

        [LoggerMessage(
            EventId = 1900,
            Level = LogLevel.Error,
            Message = "An error occured attempting to register a {ConsumerName} of type {ConsumerType}"
            )]
        public static partial void ConsumerRegistrationFailed(ILogger logger, Exception exception, string consumerName, Type consumerType);

        [LoggerMessage(
            EventId = 1901,
            Level = LogLevel.Error,
            Message = "Failed to fallback"
            )]
        public static partial void ResilienceFailedToFallback(ILogger logger, Exception exception);
    }
}
