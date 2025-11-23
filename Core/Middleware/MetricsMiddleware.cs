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

        public MetricsMiddleware(Meter? meter, bool useInternal)
        {
            if (meter!=null)
                systemTracker=new(meter!);
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

        public async ValueTask<(TMessage message, MessageHeader messageHeader)> AfterMessageDecodeAsync<TMessage>(IContext context, TMessage message, string ID, MessageHeader messageHeader, DateTime receivedTimestamp, DateTime processedTimeStamp)
        {
            await AddStat(typeof(TMessage), (string?)context[MessageReceivedChannelKey]??string.Empty, false, (int?)context[MessageReceivedSizeKey]??0, GetDuration(context));   
            context[MessageReceivedChannelKey]=null;
            context[MessageReceivedSizeKey]=null;
            return (message, messageHeader);
        }

        public async ValueTask<ServiceMessage> AfterMessageEncodeAsync(Type messageType, IContext context, ServiceMessage message)
        {
            await AddStat(messageType, message.Channel, true, message.Data.Length, GetDuration(context));
            return message;
        }

        public ValueTask<(MessageHeader messageHeader, ReadOnlyMemory<byte> data)> BeforeMessageDecodeAsync(IContext context, string id, MessageHeader messageHeader, string messageTypeID, string messageChannel, ReadOnlyMemory<byte> data)
        {
            context[MessageReceivedChannelKey] = messageChannel;
            context[MessageReceivedSizeKey] = data.Length;
            context[StopWatchKey] = Stopwatch.GetTimestamp();
            return ValueTask.FromResult((messageHeader, data));
        }

        public ValueTask<(TMessage message, string? channel, MessageHeader messageHeader)> BeforeMessageEncodeAsync<TMessage>(IContext context, TMessage message, string? channel, MessageHeader messageHeader)
        {
            context[StopWatchKey] = Stopwatch.GetTimestamp();
            return ValueTask.FromResult((message, channel, messageHeader));
        }
    }
}
