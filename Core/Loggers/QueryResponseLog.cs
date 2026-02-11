using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Loggers
{
    /// query log ids = 3000-3199
    /// subscription log ids = 3200-3399
    internal static partial class QueryResponseLog
    {
        #region Query
        public const int ExecutingQueryEventId = 3000;

        [LoggerMessage(
            EventId = ExecutingQueryEventId,
            Level = LogLevel.Debug,
            Message = "Executing QueryResponse of {TQuery}, expecting {TQueryResponse} on {Channel} with {ResponseChannel}"
            )]
        public static partial void ExecutingQuery(ILogger? logger, Type tQuery, Type tQueryResponse, string? channel, string? responseChannel);

        [LoggerMessage(
            EventId = 3001,
            Level = LogLevel.Debug,
            Message = "Attempting to get response type for QueryResponse for {TQuery} on {Channel} with {ResponseChannel}"
            )]
        public static partial void ExtractingQueryResponseType(ILogger? logger, Type tQuery, string? channel, string? responseChannel);

        [LoggerMessage(
            EventId = 3002,
            Level = LogLevel.Debug,
            Message = "Establishing an instance of Inbox Message style handling for a QueryResponse call on {ConnectionName}"
            )]
        public static partial void EstablishingInbox(ILogger? logger, string? connectionName);

        [LoggerMessage(
            EventId = 3003,
            Level = LogLevel.Information,
            Message = "Setting up Inbox Message listener with {CorrelationID}"
            )]
        public static partial void SettingUpInbox(ILogger? logger, Guid correlationID);

        [LoggerMessage(
            EventId = 3004,
            Level = LogLevel.Debug,
            Message = "Establishing new Inbox Subscription for {ConnectionName}"
            )]
        public static partial void EstablishingNewInbox(ILogger? logger, string? connectionName);

        [LoggerMessage(
            EventId = 3005,
            Level = LogLevel.Information,
            Message = "Attempting to process Inbox message with {CorrelationID}"
            )]
        public static partial void AttemptingToProcessInboxMessage(ILogger? logger, Guid correlationID);

        [LoggerMessage(
            EventId = 3006,
            Level = LogLevel.Debug,
            Message = "Inbox Query Message has timed out waiting for the response"
            )]
        public static partial void InboxQueryMessageTimedout(ILogger? logger);

        [LoggerMessage(
            EventId = 3007,
            Level = LogLevel.Information,
            Message = "Transmitting Inbox Query request to underlying system with {CorrelationID} and being waiting on response"
            )]
        public static partial void TransmittingInboxQuery(ILogger? logger, Guid correlationID);

        [LoggerMessage(
            EventId = 3008,
            Level = LogLevel.Information,
            Message = "Inbox Query tranmission failed cleaning up resources"
            )]
        public static partial void TransmittingInboxQueryFailed(ILogger? logger);

        [LoggerMessage(
            EventId = 3009,
            Level = LogLevel.Debug,
            Message = "Attempting to produce a Query Result of {TQueryResponse} from the Service Message of the type {MessageTypeID}"
            )]
        public static partial void ProcessingQueryResponse(ILogger? logger, Type tQueryResponse, string messageTypeID);

        [LoggerMessage(
            EventId = 3010,
            Level = LogLevel.Error,
            Message = "A query response exception occured"
            )]
        public static partial void QueryExceptionOccured(ILogger? logger, Exception exception);

        [LoggerMessage(
            EventId = 3011,
            Level = LogLevel.Error,
            Message = "An error occured attempting to convert the Service Message of the type {MessageTypeID} to the Query Result of {TQueryResponse}"
            )]
        public static partial void ProcessingExceptionOccured(ILogger? logger, Exception exception, string messageTypeID, Type tQueryResponse);

        [LoggerMessage(
            EventId = 3012,
            Level = LogLevel.Debug,
            Message = "Attempting to execute a Query of {TQuery} with a response {TQueryResponse}"
            )]
        public static partial void AttemptingQueryResponse(ILogger? logger, Type tQuery, Type tQueryResponse);

        [LoggerMessage(
            EventId = 3013,
            Level = LogLevel.Information,
            Message = "Executing a QueryResponse call on a QueryResponse service connection"
            )]
        public static partial void ExecutingQueryResponseOnQueryResponseService(ILogger? logger);

        [LoggerMessage(
            EventId = 3014,
            Level = LogLevel.Information,
            Message = "Executing a QueryResponse call on an InboxQuery service connection"
            )]
        public static partial void ExecutingQueryResponseOnInboxService(ILogger? logger);

        [LoggerMessage(
            EventId = 3015,
            Level = LogLevel.Information,
            Message = "Executing a QueryResponse call on a standard PubSub service connection using {ResponseChannel}"
            )]
        public static partial void ExecutingQueryResponseOnPubSubService(ILogger? logger, string? responseChannel);
        #endregion

        #region Subscription
        [LoggerMessage(
            EventId = 3200,
            Level = LogLevel.Debug,
            Message = "Creating QueryResponse subscription for message query {TQuery} and response {TQueryResponse} on channel {Channel} in group {Group}"
            )]
        public static partial void CreatingSubscription(ILogger? logger, Type tQuery, Type tQueryResponse, string? channel, string? group);

        [LoggerMessage(
            EventId = 3201,
            Level = LogLevel.Information,
            Message = "Constructing QueryResponse subscription for {TQuery} answering with {TQueryResponse} on {Channel} in {Group}"
            )]
        public static partial void ConstructingSubscription(ILogger? logger, Type tQuery, Type tQueryResponse, string? channel, string? group);

        [LoggerMessage(
            EventId = 3202,
            Level = LogLevel.Debug,
            Message = "Establishing QueryResponse subscription"
            )]
        public static partial void EstablishingSubscription(ILogger? logger);

        [LoggerMessage(
            EventId = 3203,
            Level = LogLevel.Debug,
            Message = "Failed to establish subscription"
            )]
        public static partial void EstablishingSubscriptionFailed(ILogger? logger);

        [LoggerMessage(
            EventId = 3204,
            Level = LogLevel.Information,
            Message = "Establishing underlying service subscription for QueryResponse subscription."
            )]
        public static partial void EstablishingServiceSubscription(ILogger? logger);

        [LoggerMessage(
            EventId = 3205,
            Level = LogLevel.Information,
            Message = "Establishing underlying QueryResponse service subscription."
            )]
        public static partial void EstablishingQueryResponseServiceSubscription(ILogger? logger);

        [LoggerMessage(
            EventId = 3206,
            Level = LogLevel.Information,
            Message = "Successfully established QueryResponse subscription."
            )]
        public static partial void EstablishingQueryResponseServiceSubscriptionSuccess(ILogger? logger);

        [LoggerMessage(
            EventId = 3207,
            Level = LogLevel.Information,
            Message = "Establishing underlying PubSub service subscription to listen for incoming queries."
            )]
        public static partial void EstablishingPubSubServiceSubscription(ILogger? logger);

        [LoggerMessage(
            EventId = 3208,
            Level = LogLevel.Information,
            Message = "Successfully established QueryResponse subscription."
            )]
        public static partial void EstablishinPubSubServiceSubscriptionSuccess(ILogger? logger);

        [LoggerMessage(
            EventId = 3209,
            Level = LogLevel.Error,
            Message = "Error occurred while establishing the subscription."
            )]
        public static partial void ErrorEstablishingSubscription(ILogger? logger, Exception exception);

        [LoggerMessage(
            EventId = 3300,
            Level = LogLevel.Warning,
            Message = "Received invalid query response message."
            )]
        public static partial void ReceivedInvalidQueryResponseMessage(ILogger? logger);

        [LoggerMessage(
            EventId = 3301,
            Level = LogLevel.Debug,
            Message = "Processing received service message with id {MessageID}."
            )]
        public static partial void ProcessingRecievedMessage(ILogger? logger,string messageID);

        [LoggerMessage(
            EventId = 3302,
            Level = LogLevel.Debug,
            Message = "Waiting for manual reset event to complete synchronous operation."
            )]
        public static partial void WaitingToProcessMessage(ILogger? logger);

        [LoggerMessage(
            EventId = 3303,
            Level = LogLevel.Debug,
            Message = "Processing service message with ID: {MessageID}"
            )]
        public static partial void ProcessingServiceMessage(ILogger? logger, string messageID);

        [LoggerMessage(
            EventId = 3304,
            Level = LogLevel.Debug,
            Message = "Acknowledging service message with ID: {MessageID}"
            )]
        public static partial void AcknowledgingServiceMessage(ILogger? logger, string messageID);

        [LoggerMessage(
            EventId = 3305,
            Level = LogLevel.Error,
            Message = "Error occurred while processing service message with ID: {MessageID}"
            )]
        public static partial void ErrorProcessingServiceMessage(ILogger? logger, Exception exception, string messageID);

        [LoggerMessage(
            EventId = 3306,
            Level = LogLevel.Debug,
            Message = "Setting manual reset event for synchronous operation."
            )]
        public static partial void ReleasingMessageWait(ILogger? logger);

        [LoggerMessage(
            EventId = 3307,
            Level = LogLevel.Warning,
            Message = "Returning error response for message with ID: {MessageID}"
            )]
        public static partial void ReturningErrorMessage(ILogger? logger, string messageID);

        [LoggerMessage(
            EventId = 3308,
            Level = LogLevel.Information,
            Message = "Returning valid service response for message with ID: {MessageID}"
            )]
        public static partial void ReturningValidResponse(ILogger? logger, string messageID);

        [LoggerMessage(
            EventId = 3309,
            Level = LogLevel.Information,
            Message = "Disposing resources for QueryResponseSubscription."
            )]
        public static partial void Disposing(ILogger? logger);

        [LoggerMessage(
            EventId = 3310,
            Level = LogLevel.Debug,
            Message = "Cancelling token for QueryResponseSubscription."
            )]
        public static partial void CancellingToken(ILogger? logger);

        [LoggerMessage(
            EventId = 3311,
            Level = LogLevel.Information,
            Message = "Resources for QueryResponseSubscription have been disposed."
            )]
        public static partial void Disposed(ILogger? logger);
        #endregion
    }
}
