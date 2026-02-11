using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Loggers
{
    /// publish log ids 2000-2199
    /// subscription log ids = 2200-2399
    internal static partial class PubSubLog
    {
        #region Publish
        public const int ExecutingBulkPublishEventId = 2011;

        [LoggerMessage(
            EventId = 2000,
            Level = LogLevel.Debug,
            Message = "Publishing message {MessageType} on {Channel}"
            )]
        public static partial void PublishingMessage(ILogger? logger, Type messageType, string? channel);

        [LoggerMessage(
            EventId = 2010,
            Level = LogLevel.Debug,
            Message = "Bulk Publishing messages {MessageType} on {Channel}"
            )]
        public static partial void BulkPublishingMessage(ILogger? logger, Type messageType, string? channel);

        [LoggerMessage(
            EventId = ExecutingBulkPublishEventId,
            Level = LogLevel.Debug,
            Message = "Executing bulk publish"
            )]
        public static partial void ExecutingBulkPublish(ILogger? logger);
        #endregion

        #region Subscriptions
        [LoggerMessage(
            EventId = 2200,
            Level = LogLevel.Debug,
            Message = "Creating PubSub Subscription for {MessageType} on {Channel} in {Group}."
            )]
        public static partial void CreatingPubSubSubscription(ILogger? logger, Type messageType, string? channel, string? group);

        [LoggerMessage(
            EventId = 2201,
            Level = LogLevel.Information,
            Message = "Establishing PubSub Subscription connection."
            )]
        public static partial void EstablishingPubSubSubscription(ILogger? logger);

        [LoggerMessage(
            EventId = 2202,
            Level = LogLevel.Information,
            Message = "Establishment of PubSub Subscription connection failed."
            )]
        public static partial void PubSubSubscriptionEstablishmentFailed(ILogger? logger);

        [LoggerMessage(
            EventId = 2203,
            Level = LogLevel.Debug,
            Message = "Establishing underlying service subscription for PubSub subscription."
            )]
        public static partial void EstablishingPubSubServiceSubscription(ILogger? logger);

        [LoggerMessage(
           EventId = 2204,
           Level = LogLevel.Information,
           Message = "Successfully established PubSub subscription."
           )]
        public static partial void PubSubSubscriptionEstablishmentSucceeded(ILogger? logger);

        [LoggerMessage(
           EventId = 2211,
           Level = LogLevel.Debug,
           Message = "Processing service message with ID: {MessageID}"
           )]
        public static partial void ProcessingServiceMessage(ILogger? logger,string messageID);

        [LoggerMessage(
           EventId = 2212,
           Level = LogLevel.Debug,
           Message = "Acknowledging service message with ID: {MessageID}"
           )]
        public static partial void AcknowledgingServiceMessage(ILogger? logger, string messageID);

        [LoggerMessage(
           EventId = 2213,
           Level = LogLevel.Error,
           Message = "Error occurred while processing service message with ID: {MessageID}"
           )]
        public static partial void ProcessingServiceMessageError(ILogger? logger, Exception exception, string messageID);
        #endregion
    }
}
