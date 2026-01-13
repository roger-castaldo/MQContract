using MQContract.Interfaces;
using MQContract.Interfaces.Middleware;
using MQContract.Messages;
using MQContract.Middleware.Metrics;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Channels;

namespace MQContract.Middleware
{
    [MiddlewareInjectionOrder<IBeforeEncodeMiddleware>(preIndex: 1)]
    [MiddlewareInjectionOrder<IAfterEncodeMiddleware>(postIndex: 3)]
    [MiddlewareInjectionOrder<IBeforeDecodeMiddleware>(preIndex: 1)]
    [MiddlewareInjectionOrder<IAfterDecodeMiddleware>(postIndex: 3)]
    internal class MetricsMiddleware : IBeforeEncodeMiddleware, IAfterEncodeMiddleware, IBeforeDecodeMiddleware, IAfterDecodeMiddleware
    {
        private const string StopWatchKey = "_MetricStopwatch";
        private const string MessageReceivedChannelKey = "_MetricMessageReceivedChannel";
        private const string MessageReceivedSizeKey = "_MetricMessageReceivedSize";

        private readonly SystemMetricTracker? systemTracker;
        private readonly InternalMetricTracker? internalTracker;
        private readonly Channel<MetricEntryValue> channel = Channel.CreateUnbounded<MetricEntryValue>();

        public MetricsMiddleware(Meter? meter, MessageContext context, bool useInternal)
        {
            if (meter!=null)
                systemTracker=new(meter!, context);
            if (useInternal)
                internalTracker=new();
            Start();
        }

        private void Start()
        {
            Task.Run(async () =>
            {
                while (await channel.Reader.WaitToReadAsync())
                {
                    var entry = await channel.Reader.ReadAsync();
                    if (entry != null)
                    {
                        systemTracker?.AppendEntry(entry!);
                        internalTracker?.AppendEntry(entry!);
                    }
                }
            });
        }

        public IContractMetric? GetSnapshot(bool sent)
            => internalTracker?.GetSnapshot(sent);
        public IContractMetric? GetSnapshot(Type messageType, bool sent)
            => internalTracker?.GetSnapshot(messageType, sent);
        public IContractMetric? GetSnapshot(string channel, bool sent)
            => internalTracker?.GetSnapshot(channel, sent);

        private async ValueTask AddStat(Type messageType, string? channel, bool sending, int messageSize, TimeSpan duration)
            => await this.channel.Writer.WriteAsync(new(messageType, channel, sending, messageSize, duration));

        private static TimeSpan GetDuration(IContext context)
        {
            var timestamp = (long?)context[StopWatchKey];
            var result = (timestamp.HasValue ? Stopwatch.GetElapsedTime(timestamp.Value) : TimeSpan.Zero);
            context[StopWatchKey]=null;
            return result;
        }

        async ValueTask<DecodedMessage<TMessage>> IAfterDecodeMiddleware.AfterMessageDecodeAsync<TMessage>(IContext context, string ID, DecodedMessage<TMessage> message, DateTime receivedTimestamp, DateTime processedTimeStamp)
        { 
            await AddStat(typeof(TMessage), (string?)context[MessageReceivedChannelKey]??string.Empty, false, (int?)context[MessageReceivedSizeKey]??0, GetDuration(context));   
            context[MessageReceivedChannelKey]=null;
            context[MessageReceivedSizeKey]=null;
            return message;
        }

        public async ValueTask<ServiceMessage> AfterMessageEncodeAsync(Type messageType, IContext context, ServiceMessage message)
        {
            await AddStat(messageType, message.Channel, true, message.Data.Length, GetDuration(context));
            return message;
        }

        ValueTask<DecodableMessage> IBeforeDecodeMiddleware.BeforeMessageDecodeAsync(IContext context, string id, string messageTypeID, string messageChannel, DecodableMessage message)
        {
            context[MessageReceivedChannelKey] = messageChannel;
            context[MessageReceivedSizeKey] = message.Data.Length;
            context[StopWatchKey] = Stopwatch.GetTimestamp();
            return ValueTask.FromResult(message);
        }

        ValueTask<EncodableMessage<TMessage>> IBeforeEncodeMiddleware.BeforeMessageEncodeAsync<TMessage>(IContext context, EncodableMessage<TMessage> message)
        {
            context[StopWatchKey] = Stopwatch.GetTimestamp();
            return ValueTask.FromResult(message);
        }
    }
}
