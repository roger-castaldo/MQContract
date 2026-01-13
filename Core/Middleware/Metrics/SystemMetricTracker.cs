using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace MQContract.Middleware.Metrics
{
    internal class SystemMetricTracker
    {
        private const string MeterName = "mqcontract";

        private readonly Meter meter;
        private readonly MessageContext context;
        private readonly MessageMetric globalMetric;
        private readonly ConcurrentDictionary<Type, MessageMetric> typeMetrics = [];
        private readonly ConcurrentDictionary<string, MessageMetric> channelMetrics = [];

        public SystemMetricTracker(Meter meter, MessageContext context)
        {
            this.meter = meter;
            this.context = context;
            globalMetric = new(
                meter.CreateUpDownCounter<long>($"{MeterName}.messages.sent.count"),
                meter.CreateUpDownCounter<long>($"{MeterName}.messages.sent.bytes"),
                meter.CreateUpDownCounter<long>($"{MeterName}.messages.received.count"),
                meter.CreateUpDownCounter<long>($"{MeterName}.messages.received.bytes"),
                meter.CreateHistogram<double>($"{MeterName}.messages.encodingduration", unit: "ms"),
                meter.CreateHistogram<double>($"{MeterName}.messages.decodingduration", unit: "ms")
            );
        }

        public void AppendEntry(MetricEntryValue entry)
        {
            globalMetric.AddEntry(entry);
            MessageMetric? channelMetric = null;
            if (!typeMetrics.TryGetValue(entry.Type, out MessageMetric? typeMetric))
            {
                typeMetric = new(
                    meter.CreateUpDownCounter<long>($"{MeterName}.types.{context.MessageTypeName(entry.Type)}.{context.MessageVersionString(entry.Type).Replace('.', '_')}.sent.count"),
                    meter.CreateUpDownCounter<long>($"{MeterName}.types.{context.MessageTypeName(entry.Type)}.{context.MessageVersionString(entry.Type).Replace('.', '_')}.sent.bytes"),
                    meter.CreateUpDownCounter<long>($"{MeterName}.types.{context.MessageTypeName(entry.Type)}.{context.MessageVersionString(entry.Type).Replace('.', '_')}.received.count"),
                    meter.CreateUpDownCounter<long>($"{MeterName}.types.{context.MessageTypeName(entry.Type)}.{context.MessageVersionString(entry.Type).Replace('.', '_')}.received.bytes"),
                    meter.CreateHistogram<double>($"{MeterName}.types.{context.MessageTypeName(entry.Type)}.{context.MessageVersionString(entry.Type).Replace('.', '_')}.encodingduration", unit: "ms"),
                    meter.CreateHistogram<double>($"{MeterName}.types.{context.MessageTypeName(entry.Type)}.{context.MessageVersionString(entry.Type).Replace('.', '_')}.decodingduration", unit: "ms")
                );
                typeMetrics.TryAdd(entry.Type, typeMetric!);
            }
            if (!string.IsNullOrWhiteSpace(entry.Channel) && !channelMetrics.TryGetValue(entry.Channel, out channelMetric))
            {
                channelMetric = new(
                    meter.CreateUpDownCounter<long>($"{MeterName}.channels.{entry.Channel}.sent.count"),
                    meter.CreateUpDownCounter<long>($"{MeterName}.channels.{entry.Channel}.sent.bytes"),
                    meter.CreateUpDownCounter<long>($"{MeterName}.channels.{entry.Channel}.received.count"),
                    meter.CreateUpDownCounter<long>($"{MeterName}.channels.{entry.Channel}.received.bytes"),
                    meter.CreateHistogram<double>($"{MeterName}.channels.{entry.Channel}.encodingduration", unit: "ms"),
                    meter.CreateHistogram<double>($"{MeterName}.channels.{entry.Channel}.decodingduration", unit: "ms")
                );
                channelMetrics.TryAdd(entry.Channel!, channelMetric!);
            }
            typeMetric?.AddEntry(entry);
            channelMetric?.AddEntry(entry);
        }
    }
}
