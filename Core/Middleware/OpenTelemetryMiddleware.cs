using MQContract.Interfaces.Middleware;
using MQContract.Messages;
using System.Diagnostics;

namespace MQContract.Middleware
{
    internal class OpenTelemetryMiddleware() 
        : IBeforeEncodeMiddleware, IAfterEncodeMiddleware, IBeforeDecodeMiddleware, IAfterDecodeMiddleware
    {
        private const string KeyBase = "mqcontract";
        private const string InitialChannelKey = $"{KeyBase}.initialchannel";
        private const string TransmissionChannelKey = $"{KeyBase}.transmissionchannel";
        private const string RecievedChannelKey = $"{KeyBase}.recievedchannel";
        private const string MessageTypeClassKey = $"{KeyBase}.messagetypeclass";
        private const string MessageTypeKey = $"{KeyBase}.messagetypetag";
        private const string EncodingDurationKey = $"{KeyBase}.messageencodingduration";
        private const string MessageIdKey = $"{KeyBase}.messageid";
        private const string StopwatchContextId = "_TelemetryStopwatch";

        ValueTask<(T message, string? channel, MessageHeader messageHeader)> IBeforeEncodeMiddleware.BeforeMessageEncodeAsync<T>(IContext context, T message, string? channel, MessageHeader messageHeader)
        {
            context.Activity?.AddTag(InitialChannelKey,channel);
            context.Activity?.AddTag(MessageTypeClassKey, typeof(T).Name);
            context[StopwatchContextId] = Stopwatch.GetTimestamp();
            return ValueTask.FromResult((message, channel, messageHeader));
        }

        ValueTask<ServiceMessage> IAfterEncodeMiddleware.AfterMessageEncodeAsync(Type messageType, IContext context, ServiceMessage message)
        {
            context.Activity?.AddTag(MessageTypeKey, message.MessageTypeID);
            context.Activity?.AddTag(TransmissionChannelKey, message.Channel);
            context.Activity?.AddTag(MessageIdKey, message.ID);
            if (context[StopwatchContextId]!=null)
                context.Activity?.AddEvent(new("MessageEncoded", tags: new([
                    new(EncodingDurationKey, $"{Stopwatch.GetElapsedTime((long)context[StopwatchContextId]!).TotalMilliseconds}ms"),
                    new(MessageIdKey,message.ID)
                ])));
            return ValueTask.FromResult(message);
        }

        ValueTask<(MessageHeader messageHeader, ReadOnlyMemory<byte> data)> IBeforeDecodeMiddleware.BeforeMessageDecodeAsync(IContext context, string id, MessageHeader messageHeader, string messageTypeID, string messageChannel, ReadOnlyMemory<byte> data)
        {
            context.Activity?.AddTag(MessageTypeKey, messageTypeID);
            context.Activity?.AddTag(RecievedChannelKey, messageChannel);
            context.Activity?.AddTag(MessageIdKey, id);
            context[StopwatchContextId] = Stopwatch.GetTimestamp();
            return ValueTask.FromResult((messageHeader, data));
        }

        ValueTask<(T message, MessageHeader messageHeader)> IAfterDecodeMiddleware.AfterMessageDecodeAsync<T>(IContext context, T message, string ID, MessageHeader messageHeader, DateTime receivedTimestamp, DateTime processedTimeStamp)
        {
            context.Activity?.AddTag(MessageTypeClassKey, typeof(T).Name);
            if (context[StopwatchContextId]!=null)
                context.Activity?.AddEvent(new("MessageDecoded", tags: new([new(EncodingDurationKey, $"{Stopwatch.GetElapsedTime((long)context[StopwatchContextId]!).TotalMilliseconds}ms")])));
            return ValueTask.FromResult((message, messageHeader));
        }
    }
}
