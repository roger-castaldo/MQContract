using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Logging
{
    internal static partial class Logs
    {
        internal static partial class Pipeline
        {
            [LoggerMessage(EventId = EventIds.Pipeline.ResilienceRetryTriggered,Level = LogLevel.Debug,Message = "Retry fallback has been triggered for {MessageID}")]
            public static partial void ResilienceRetryTriggered(ILogger logger, string messageID);

            [LoggerMessage(EventId = EventIds.Pipeline.ExecutingBeforeMessageEncodeMiddleware,Level = LogLevel.Debug,Message = "Executing generic Before Message Encode middleware for message of type {Type}")]
            public static partial void ExecutingGenericBeforeEncodeMiddleware(ILogger logger, Type type);

            [LoggerMessage(EventId = EventIds.Pipeline.ExecutingSpecificBeforeMessageEncodeMiddleware,Level = LogLevel.Debug,Message = "Executing specific for type Before Message Encode middleware for message of type {Type}")]
            public static partial void ExecutingSpecificBeforeEncodeMiddleware(ILogger logger, Type type);

            [LoggerMessage(EventId = EventIds.Pipeline.ExecutingAfterMessageEncodeMiddleware,Level = LogLevel.Debug,Message = "Executing generic After Message Encode middleware for message of type {Type}")]
            public static partial void ExecutingGenericAfterEncodeMiddleware(ILogger logger, Type type);

            [LoggerMessage(EventId = EventIds.Pipeline.ExecutingBeforeMessageDecodeMiddleware,Level = LogLevel.Debug,Message = "Executing generic Before Message Decode middleware")]
            public static partial void ExecutingGenericBeforeDecodeMiddleware(ILogger logger);

            [LoggerMessage(EventId = EventIds.Pipeline.ExecutingAfterMessageDecodeMiddleware,Level = LogLevel.Debug,Message = "Executing generic After Message Decode middleware for message of type {Type}")]
            public static partial void ExecutingGenericAfterDecodeMiddleware(ILogger logger, Type type);

            [LoggerMessage(EventId = EventIds.Pipeline.ExecutingSpecificAfterMessageDecodeMiddleware,Level = LogLevel.Debug,Message = "Executing specific for type After Message Decode middleware for message of type {Type}")]
            public static partial void ExecutingSpecificAfterDecodeMiddleware(ILogger logger, Type type);

            [LoggerMessage(EventId = EventIds.Pipeline.ProducingServiceMessage,Level = LogLevel.Debug,Message = "Producing Service Message for message of type {Type}")]
            public static partial void ProducingServiceMessage(ILogger logger, Type type);

            [LoggerMessage(EventId = EventIds.Pipeline.FilteringServiceMessageByHeaders,Level = LogLevel.Debug,Message = "Filtering Service Message message of type {Type} by headers")]
            public static partial void FilteringServiceMessageByHeaders(ILogger logger, Type type);

            [LoggerMessage(EventId = EventIds.Pipeline.DecodingServiceMessage,Level = LogLevel.Debug,Message = "Decoding Service Message message of type {Type}")]
            public static partial void DecodingServiceMessage(ILogger logger, Type type);

            [LoggerMessage(EventId = EventIds.Pipeline.FilterServiceMessageInFull,Level = LogLevel.Debug,Message = "Filtering Service Message message of type {Type} in full")]
            public static partial void FilteringServiceMessageInFull(ILogger logger, Type type);

            [LoggerMessage(EventId = EventIds.Pipeline.ResilienceFailedToFallback,Level = LogLevel.Error,Message = "Failed to fallback")]
            public static partial void ResilienceFailedToFallback(ILogger logger, Exception exception);
            
            [LoggerMessage(EventId = EventIds.Pipeline.ExtractingQueryResponseType,Level = LogLevel.Debug,Message = "Attempting to get response type for QueryResponse for {TQuery} on {Channel} with {ResponseChannel}")]
            public static partial void ExtractingQueryResponseType(ILogger logger, Type tQuery, string? channel, string? responseChannel);

            [LoggerMessage(EventId = EventIds.Pipeline.AttemptingToProcessInboxMessage,Level = LogLevel.Information,Message = "Attempting to process Inbox message with {CorrelationID}")]
            public static partial void AttemptingToProcessInboxMessage(ILogger logger, Guid correlationID);
            
            [LoggerMessage(EventId = EventIds.Pipeline.ProcessingQueryResponse,Level = LogLevel.Debug,Message = "Attempting to produce a Query Result of {TQueryResponse} from the Service Message of the type {MessageTypeID}")]
            public static partial void ProcessingQueryResponse(ILogger logger, Type tQueryResponse, string messageTypeID);

            [LoggerMessage(EventId = EventIds.Pipeline.ProcessingQueryResponseFailed,Level = LogLevel.Error,Message = "An error occured attempting to convert the Service Message of the type {MessageTypeID} to the Query Result of {TQueryResponse}")]
            public static partial void ProcessingExceptionOccured(ILogger logger, Exception exception, string messageTypeID, Type tQueryResponse);
            
            [LoggerMessage(EventId = EventIds.Pipeline.QueryReplyChannelMapped,Level = LogLevel.Debug,Message = "QueryResponse reply channel mapped to {ReplyChannel}")]
            public static partial void ReplyChannelMapped(ILogger logger, string replyChannel);
            
            [LoggerMessage(EventId = EventIds.Pipeline.ReceivedInvalidQueryResponseMessage,Level = LogLevel.Warning,Message = "Received invalid query response message.")]
            public static partial void ReceivedInvalidQueryResponseMessage(ILogger logger);
        }
    }
}
