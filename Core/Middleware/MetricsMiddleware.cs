using MQContract.Interfaces;
using MQContract.Interfaces.Middleware;
using MQContract.Messages;
using MQContract.Middleware.Metrics;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Channels;

namespace MQContract.Middleware
{
    internal class MetricsMiddleware : IBeforeEncodeMiddleware, IAfterEncodeMiddleware,IBeforeDecodeMiddleware,IAfterDecodeMiddleware
    {
        private const string StopWatchKey = "_MetricStopwatch";
        private const string MessageReceivedChannelKey = "_MetricMessageReceivedChannel";
        private const string MessageRecievedSizeKey = "_MetricMessageRecievedSize";

        private readonly SystemMetricTracker? systemTracker;
        private readonly InternalMetricTracker? internalTracker;
        private readonly Channel<MetricEntryValue> channel = Channel.CreateUnbounded<MetricEntryValue>();

        public MetricsMiddleware(Meter? meter,bool useInternal)
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
            => internalTracker?.GetSnapshot(channel,sent);

        private async ValueTask AddStat(Type messageType, string? channel, bool sending, int messageSize, Stopwatch? stopWatch)
            => await this.channel.Writer.WriteAsync(new(messageType, channel, sending, messageSize, stopWatch?.Elapsed??TimeSpan.Zero));
        
        public async ValueTask<(T message, MessageHeader messageHeader)> AfterMessageDecodeAsync<T>(IContext context, T message, string ID, MessageHeader messageHeader, DateTime receivedTimestamp, DateTime processedTimeStamp)
        {
            var stopWatch = (Stopwatch?)context[StopWatchKey];
            stopWatch?.Stop();
            await AddStat(typeof(T), (string?)context[MessageReceivedChannelKey]??string.Empty, false, (int?)context[MessageRecievedSizeKey]??0, stopWatch);
            context[StopWatchKey]=null;
            context[MessageReceivedChannelKey]=null;
            context[MessageRecievedSizeKey]=null;
            return (message,messageHeader);
        }

        public async ValueTask<ServiceMessage> AfterMessageEncodeAsync(Type messageType, IContext context, ServiceMessage message)
        {
            var stopWatch = (Stopwatch?)context[StopWatchKey];
            stopWatch?.Stop();
            await AddStat(messageType, message.Channel,true,message.Data.Length,stopWatch);
            context[StopWatchKey] = null;
            return message;
        }

        public ValueTask<(MessageHeader messageHeader, ReadOnlyMemory<byte> data)> BeforeMessageDecodeAsync(IContext context, string id, MessageHeader messageHeader, string messageTypeID,string messageChannel, ReadOnlyMemory<byte> data)
        {
            context[MessageReceivedChannelKey] = messageChannel;
            context[MessageRecievedSizeKey] = data.Length;
            var stopwatch = new Stopwatch();
            context[StopWatchKey] = stopwatch;
            stopwatch.Start();
            return ValueTask.FromResult((messageHeader, data));
        }

        public ValueTask<(T message, string? channel, MessageHeader messageHeader)> BeforeMessageEncodeAsync<T>(IContext context, T message, string? channel, MessageHeader messageHeader)
        {
            var stopwatch = new Stopwatch();
            context[StopWatchKey] = stopwatch;
            stopwatch.Start();
            return ValueTask.FromResult((message, channel, messageHeader));
        }
    }
}
