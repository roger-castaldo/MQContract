using System.Diagnostics.Metrics;

namespace MQContract.Middleware.Metrics
{
    internal record MessageMetric(UpDownCounter<long> Sent, UpDownCounter<long> SentBytes, UpDownCounter<long> Received, UpDownCounter<long> ReceivedBytes, 
        Histogram<double> EncodingDuration, Histogram<double> DecodingDuration)
    {
        public void AddEntry(MetricEntryValue entry)
        {
            if (entry.Sent)
            {
                Sent.Add(1);
                SentBytes.Add(entry.MessageSize);
                EncodingDuration.Record(entry.Duration.TotalMilliseconds);
            }
            else
            {
                Received.Add(1);
                ReceivedBytes.Add(entry.MessageSize);
                DecodingDuration.Record(entry.Duration.TotalMilliseconds);
            }
        }
    }
}
