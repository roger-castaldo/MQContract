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

        public SystemMetricTracker()
        {
            meter = new Meter(MeterName);
            globalMetric = new(
                meter.CreateCounter<long>($"{MeterName}.messages.sent.count"),
                meter.CreateCounter<long>($"{MeterName}.messages.sent.bytes"),
                meter.CreateCounter<long>($"{MeterName}.messages.received.count"),
                meter.CreateCounter<long>($"{MeterName}.messages.received.bytes"),
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
                    meter.CreateCounter<long>($"{MeterName}.types.{Utility.MessageTypeName(entry.Type)}.{Utility.MessageVersionString(entry.Type).Replace('.','_')}.sent.count"),
                    meter.CreateCounter<long>($"{MeterName}.types.{Utility.MessageTypeName(entry.Type)}.{Utility.MessageVersionString(entry.Type).Replace('.', '_')}.sent.bytes"),
                    meter.CreateCounter<long>($"{MeterName}.types.{Utility.MessageTypeName(entry.Type)}.{Utility.MessageVersionString(entry.Type).Replace('.', '_')}.received.count"),
                    meter.CreateCounter<long>($"{MeterName}.types.{Utility.MessageTypeName(entry.Type)}.{Utility.MessageVersionString(entry.Type).Replace('.', '_')}.received.bytes"),
                    meter.CreateHistogram<double>($"{MeterName}.types.{Utility.MessageTypeName(entry.Type)}.{Utility.MessageVersionString(entry.Type).Replace('.', '_')}.encodingduration"),
                    meter.CreateHistogram<double>($"{MeterName}.types.{Utility.MessageTypeName(entry.Type)}.{Utility.MessageVersionString(entry.Type).Replace('.', '_')}.decodingduration")
                );
                typeMetrics.Add(entry.Type, typeMetric!);
            }
            if (!string.IsNullOrWhiteSpace(entry.Channel) && !channelMetrics.TryGetValue(entry.Channel, out channelMetric))
            {
                channelMetric = new(
                    meter.CreateCounter<long>($"{MeterName}.channels.{entry.Channel}.sent.count"),
                    meter.CreateCounter<long>($"{MeterName}.channels.{entry.Channel}.sent.bytes"),
                    meter.CreateCounter<long>($"{MeterName}.channels.{entry.Channel}.received.count"),
                    meter.CreateCounter<long>($"{MeterName}.channels.{entry.Channel}.received.bytes"),
                    meter.CreateHistogram<double>($"{MeterName}.channels.{entry.Channel}.encodingduration"),
                    meter.CreateHistogram<double>($"{MeterName}.channels.{entry.Channel}.decodingduration")
                );
                channelMetrics.Add(entry.Channel!, channelMetric!);
            }
            typeMetric?.AddEntry(entry);
            channelMetric?.AddEntry(entry);   
            semDataLock.Release();
        }
    }
}
