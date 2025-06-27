using MQContract.Interfaces.Middleware;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Diagnostics;

namespace MQContract.Middleware
{
    [MiddlewareInjectionOrder<IBeforeEncodeMiddleware>(preIndex: 1)]
    [MiddlewareInjectionOrder<IAfterEncodeMiddleware>(postIndex: 3)]
    [MiddlewareInjectionOrder<IBeforeDecodeMiddleware>(preIndex: 1)]
    [MiddlewareInjectionOrder<IAfterDecodeMiddleware>(postIndex: 3)]
    internal class OpenTelemetryMiddleware(string sourceName,bool linkActivitiesAcrossSystems)
        : IBeforeEncodeMiddleware, IAfterEncodeMiddleware, IBeforeDecodeMiddleware, IAfterDecodeMiddleware
    {
        private readonly ActivitySource activitySource = new(sourceName);

        private const string TraceParentHeaderKey = "_traceParentId";
        private const string TraceParentSpanHeaderKey = "_traceParentSpanId";

        public const string KeyBase = "mqcontract";
        public const string InitialChannelKey = $"{KeyBase}.initialchannel";
        private const string TransmissionChannelKey = $"{KeyBase}.transmissionchannel";
        private const string RecievedChannelKey = $"{KeyBase}.recievedchannel";
        private const string MessageTypeClassKey = $"{KeyBase}.messagetypeclass";
        private const string MessageTypeKey = $"{KeyBase}.messagetypetag";
        private const string EncodingDurationKey = $"{KeyBase}.messageencodingduration";
        private const string DecodingDurationKey = $"{KeyBase}.messagedecodingduration";
        public const string MessageIdKey = $"{KeyBase}.messageid";
        private const string StopwatchContextId = "_TelemetryStopwatch";
        private const string MessagePublishStatusKey = $"{KeyBase}.status";
        private const string ConnectionNameKey = $"{KeyBase}.serviceconnectionname";
        private const string ConnectionTypeKey = $"{KeyBase}.serviceconnectiontype";

        #region middleware

        ValueTask<(T message, string? channel, MessageHeader messageHeader)> IBeforeEncodeMiddleware.BeforeMessageEncodeAsync<T>(IContext context, T message, string? channel, MessageHeader messageHeader)
        {
            if (linkActivitiesAcrossSystems && context.Activity!=null)
            {
                messageHeader = new(messageHeader, new Dictionary<string, string?>([
                    new(TraceParentHeaderKey, context.Activity.TraceId.ToString()),
                    new(TraceParentSpanHeaderKey, context.Activity.SpanId.ToString())
                ]));
            }
            context.Activity?.AddTag(InitialChannelKey, channel);
            context.Activity?.AddTag(MessageTypeClassKey, typeof(T).Name);
            context[StopwatchContextId] = Stopwatch.GetTimestamp();
            return ValueTask.FromResult((message, channel, messageHeader));
        }

        ValueTask<ServiceMessage> IAfterEncodeMiddleware.AfterMessageEncodeAsync(Type messageType, IContext context, ServiceMessage message)
        {
            context.Activity?.AddTag(MessageTypeKey, message.MessageTypeID);
            context.Activity?.AddTag(TransmissionChannelKey, message.Channel);
            context.Activity?.AddTag(MessageIdKey, message.ID);
            context.Activity?.AddEvent(new("MessageEncoded", tags: new([
                new(EncodingDurationKey, context[StopwatchContextId]!=null
                    ? $"{Stopwatch.GetElapsedTime((long)context[StopwatchContextId]!).TotalMilliseconds}ms"
                    : "Unknown"
                ),
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
            context.Activity?.AddEvent(new("MessageDecoded", tags: new([
                new(DecodingDurationKey, context[StopwatchContextId]!=null
                    ? $"{Stopwatch.GetElapsedTime((long)context[StopwatchContextId]!).TotalMilliseconds}ms"
                    : "Unknown"
                ),
                new(MessageIdKey,ID)
            ])));
            return ValueTask.FromResult((message, messageHeader));
        }

        #endregion

        #region OtelHelpers
        public Activity? StartActivity(string name, MessageHeader? messageHeader = null, IMessageServiceConnection? serviceConnection = null, string? connectionName = null, Activity? current = null)
        {
            ActivityContext parent = default;
            if (!string.IsNullOrWhiteSpace(messageHeader?[TraceParentHeaderKey]))
                parent=new ActivityContext(ActivityTraceId.CreateFromString(messageHeader![TraceParentHeaderKey]!), ActivitySpanId.CreateFromString(messageHeader![TraceParentSpanHeaderKey]), ActivityTraceFlags.Recorded);
            else if (current!=null)
                parent = new ActivityContext(current.TraceId, current.SpanId, ActivityTraceFlags.Recorded);
            var activityKind = name switch
            {
                Constants.PublishActivityName => ActivityKind.Producer,
                Constants.BulkPublishActivityName => ActivityKind.Producer,
                Constants.PublishQueryActivityName => ActivityKind.Producer,
                Constants.ConsumeActivityName => ActivityKind.Consumer,
                Constants.ConsumeQueryActivityName => ActivityKind.Consumer,
                Constants.ProduceQueryResponseActivityName => ActivityKind.Producer,
                Constants.ConsumeQueryResponseActivityName => ActivityKind.Consumer,
                _ => ActivityKind.Internal
            };
            var activity = activitySource.StartActivity(name, activityKind, parent,
                links: Activity.Current!=null
                ? [new ActivityLink(ActivityContext.Parse(Activity.Current.ParentId??Activity.Current.Id!, Activity.Current!.TraceStateString))]
                : []);
            if (serviceConnection!=null)
                AssignConnectionType(activity, serviceConnection!, connectionName);
            return activity;
        }

        public static void AssignConnectionType(Activity? activity, IMessageServiceConnection serviceConnection, string? connectionName = null)
        {
            activity?.SetTag(ConnectionTypeKey, serviceConnection.GetType().FullName);
            if (connectionName != null)
                activity?.SetTag(ConnectionNameKey, connectionName);
        }

        public static KeyValuePair<string, object?> CreateConnectionTypeTag(IMessageServiceConnection serviceConnection)
            => new(ConnectionTypeKey, serviceConnection.GetType().FullName);

        public static KeyValuePair<string, object?> CreateMessagePublishStatusTag(TransmissionResult? transmissionResult)
            => new(MessagePublishStatusKey, (transmissionResult?.IsError??true ? "Fail" : "Success"));

        public static void AddMessagePublishedEvent(Activity? activity, ServiceMessage serviceMessage, TransmissionResult result, IMessageServiceConnection serviceConnection, string? connectionName = null)
            => activity?.AddEvent(new("MessagePublished", tags: new([
                new(OpenTelemetryMiddleware.MessageIdKey,serviceMessage.ID),
                new(MessagePublishStatusKey,(result.IsError ? "Fail" : "Success")),
                CreateConnectionTypeTag(serviceConnection),
                new(ConnectionNameKey,connectionName)
            ])));
        #endregion
    }
}
