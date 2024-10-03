using System.Diagnostics.Metrics;

namespace MQContract.Middleware.Metrics
{
    internal class SystemMetricTracker
    {
        private const string MeterName = "mqcontract";


        private readonly SemaphoreSlim semDataLock = new(1, 1);
        private readonly Meter meter;
        private readonly MessageMetric globalMetric;
        private readonly Dictionary<Type, MessageMetric> typeMetrics = [];
        private readonly Dictionary<string, MessageMetric> channelMetrics = [];

        public SystemMetricTracker(Meter meter)
        {
            this.meter = meter;
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
            semDataLock.Wait();
            MessageMetric? channelMetric = null;
            if (!typeMetrics.TryGetValue(entry.Type, out MessageMetric? typeMetric))
            {
                typeMetric = new(
                    meter.CreateUpDownCounter<long>($"{MeterName}.types.{Utility.MessageTypeName(entry.Type)}.{Utility.MessageVersionString(entry.Type).Replace('.','_')}.sent.count"),
                    meter.CreateUpDownCounter<long>($"{MeterName}.types.{Utility.MessageTypeName(entry.Type)}.{Utility.MessageVersionString(entry.Type).Replace('.', '_')}.sent.bytes"),
                    meter.CreateUpDownCounter<long>($"{MeterName}.types.{Utility.MessageTypeName(entry.Type)}.{Utility.MessageVersionString(entry.Type).Replace('.', '_')}.received.count"),
                    meter.CreateUpDownCounter<long>($"{MeterName}.types.{Utility.MessageTypeName(entry.Type)}.{Utility.MessageVersionString(entry.Type).Replace('.', '_')}.received.bytes"),
                    meter.CreateHistogram<double>($"{MeterName}.types.{Utility.MessageTypeName(entry.Type)}.{Utility.MessageVersionString(entry.Type).Replace('.', '_')}.encodingduration", unit: "ms"),
                    meter.CreateHistogram<double>($"{MeterName}.types.{Utility.MessageTypeName(entry.Type)}.{Utility.MessageVersionString(entry.Type).Replace('.', '_')}.decodingduration", unit: "ms")
                );
                typeMetrics.Add(entry.Type, typeMetric!);
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
                channelMetrics.Add(entry.Channel!, channelMetric!);
            }
            typeMetric?.AddEntry(entry);
            channelMetric?.AddEntry(entry);   
            semDataLock.Release();
        }
    }
}
