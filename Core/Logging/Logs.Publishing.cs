using Microsoft.Extensions.Logging;

namespace MQContract.Logging;

internal static partial class Logs
{
    internal static partial class Publishing {
        
        [LoggerMessage(EventId = EventIds.Publishing.PublishingMessage,Level = LogLevel.Debug,Message = "Publishing message {MessageType} on {Channel}")]
        public static partial void PublishingMessage(ILogger logger, Type messageType, string? channel);

        [LoggerMessage(EventId = EventIds.Publishing.BulkPublishingMessages,Level = LogLevel.Debug,Message = "Bulk Publishing messages {MessageType} on {Channel}")]
        public static partial void BulkPublishingMessages(ILogger logger, Type messageType, string? channel);

        [LoggerMessage(EventId = EventIds.Publishing.ExecutingBulkPublish,Level = LogLevel.Debug,Message = "Executing bulk publish")]
        public static partial void ExecutingBulkPublish(ILogger logger);
        
        [LoggerMessage(EventId = EventIds.Publishing.ExecutingQuery,Level = LogLevel.Debug,Message = "Executing QueryResponse of {TQuery}, expecting {TQueryResponse} on {Channel} with {ResponseChannel}")]
        public static partial void ExecutingQuery(ILogger logger, Type tQuery, Type tQueryResponse, string? channel, string? responseChannel);

        [LoggerMessage(EventId = EventIds.Publishing.InboxQueryTimedOut,Level = LogLevel.Debug,Message = "Inbox Query Message has timed out waiting for the response")]
        public static partial void InboxQueryMessageTimedout(ILogger logger);

        [LoggerMessage(EventId = EventIds.Publishing.TransmittingInboxQuery,Level = LogLevel.Information,Message = "Transmitting Inbox Query request to underlying system with {CorrelationID} and being waiting on response")]
        public static partial void TransmittingInboxQuery(ILogger logger, Guid correlationID);

        [LoggerMessage(EventId = EventIds.Publishing.TransmittingInboxQueryFailed,Level = LogLevel.Error,Message = "Inbox Query tranmission failed cleaning up resources")]
        public static partial void TransmittingInboxQueryFailed(ILogger logger);

        [LoggerMessage(EventId = EventIds.Publishing.QueryExceptionOccured,Level = LogLevel.Error,Message = "A query response exception occured")]
        public static partial void QueryExceptionOccured(ILogger logger, Exception exception);

        [LoggerMessage(EventId = EventIds.Publishing.AttemptingQueryResponse,Level = LogLevel.Debug,Message = "Attempting to execute a Query of {TQuery} with a response {TQueryResponse}")]
        public static partial void AttemptingQueryResponse(ILogger logger, Type tQuery, Type tQueryResponse);

        [LoggerMessage(EventId = EventIds.Publishing.ExecutingQueryResponseOnQueryResponseService,Level = LogLevel.Information,Message = "Executing a QueryResponse call on a QueryResponse service connection")]
        public static partial void ExecutingQueryResponseOnQueryResponseService(ILogger logger);

        [LoggerMessage(EventId = EventIds.Publishing.ExecutingQueryResponseOnInboxService,Level = LogLevel.Information,Message = "Executing a QueryResponse call on an InboxQuery service connection")]
        public static partial void ExecutingQueryResponseOnInboxService(ILogger logger);

        [LoggerMessage(EventId = EventIds.Publishing.ExecutingQueryResponseOnPubSubService,Level = LogLevel.Information,Message = "Executing a QueryResponse call on a standard PubSub service connection using {ResponseChannel}")]
        public static partial void ExecutingQueryResponseOnPubSubService(ILogger logger, string? responseChannel);

        [LoggerMessage(EventId = EventIds.Publishing.AttemptingQueryResponseOnPubSubService,Level = LogLevel.Information,Message = "Attempting a QueryResponse call using PubSub style messaging, querying {TQuery}, expecting a response of {TQueryResponse} on {ResponseChannel}")]
        public static partial void AttemptingQueryResponseOnPubSubService(ILogger logger, Type tQuery, Type tQueryResponse, string? responseChannel);

        [LoggerMessage(EventId = EventIds.Publishing.StartingQueryResponsePubSubListener,Level = LogLevel.Debug,Message = "Starting Response listener for Query over PubSub waiting on a message with {CallID}")]
        public static partial void StartingResponseListener(ILogger logger, Guid callID);

        [LoggerMessage(EventId = EventIds.Publishing.TransmittingPubSubQuery,Level = LogLevel.Debug,Message = "Transmitting Query request over PubSub")]
        public static partial void TransmittingPubSubQuery(ILogger logger);

        [LoggerMessage(EventId = EventIds.Publishing.TransmittingPubSubQueryFailed,Level = LogLevel.Error,Message = "PubSub Query tranmission failed cleaning up resources")]
        public static partial void TransmittingPubSubQueryFailed(ILogger logger);

        [LoggerMessage(EventId = EventIds.Publishing.WaitingOnPubSubQueryResponse,Level = LogLevel.Debug,Message = "Waiting on Query Response over PubSub")]
        public static partial void WaitingOnPubSubQueryResponse(ILogger logger);

        [LoggerMessage(EventId = EventIds.Publishing.PubSubQueryResponseRecieved,Level = LogLevel.Debug,Message = "Query Response over PubSub recieved")]
        public static partial void PubSubQueryResponseRecieved(ILogger logger);
    }
}
