using Microsoft.Extensions.Logging;

namespace MQContract.Logging;

internal static partial class Logs
{
    internal static partial class Consuming {
        
        [LoggerMessage(EventId = EventIds.Consuming.ProcessingServiceMessage,Level = LogLevel.Debug,Message = "Processing service message with ID: {MessageID}")]
        public static partial void ProcessingServiceMessage(ILogger logger, string messageID);

        [LoggerMessage(EventId = EventIds.Consuming.AcknowledgingServiceMessage,Level = LogLevel.Debug,Message = "Acknowledging service message with ID: {MessageID}")]
        public static partial void AcknowledgingServiceMessage(ILogger logger, string messageID);

        [LoggerMessage(EventId = EventIds.Consuming.ProcessingServiceMessageFailed,Level = LogLevel.Error,Message = "Error occurred while processing service message with ID: {MessageID}")]
        public static partial void ProcessingServiceMessageError(ILogger logger, Exception exception, string messageID);

        [LoggerMessage(EventId = EventIds.Consuming.WaitingToProcessMessage,Level = LogLevel.Debug,Message = "Waiting for manual reset event to complete synchronous operation.")]
        public static partial void WaitingToProcessMessage(ILogger logger);

        [LoggerMessage(EventId = EventIds.Consuming.ReleasingMessageWait,Level = LogLevel.Debug,Message = "Setting manual reset event for synchronous operation.")]
        public static partial void ReleasingMessageWait(ILogger logger);

        [LoggerMessage(EventId = EventIds.Consuming.ReturningErrorMessage,Level = LogLevel.Warning,Message = "Returning error response for message with ID: {MessageID}")]
        public static partial void ReturningErrorMessage(ILogger logger, string messageID);

        [LoggerMessage(EventId = EventIds.Consuming.ReturningValidResponse,Level = LogLevel.Information,Message = "Returning valid service response for message with ID: {MessageID}")]
        public static partial void ReturningValidResponse(ILogger logger, string messageID);

        [LoggerMessage(EventId = EventIds.Consuming.CancellingSubscriptionToken,Level = LogLevel.Debug,Message = "Cancelling token for QueryResponseSubscription.")]
        public static partial void CancellingToken(ILogger logger);

    }
}
