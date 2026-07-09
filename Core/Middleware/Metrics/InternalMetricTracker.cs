using MQContract.Interfaces;
using System.Collections.Concurrent;

namespace MQContract.Middleware.Metrics;

internal class InternalMetricTracker
{
    private readonly ContractMetric sentGlobalMetric = new();
    private readonly ContractMetric receivedGlobalMetric = new();
    private readonly ConcurrentDictionary<Type, ContractMetric> sentTypeMetrics = [];
    private readonly ConcurrentDictionary<Type, ContractMetric> receivedTypeMetrics = [];
    private readonly ConcurrentDictionary<string, ContractMetric> sentChannelMetrics = [];
    private readonly ConcurrentDictionary<string, ContractMetric> receivedChannelMetrics = [];

    public void AppendEntry(MetricEntryValue entry)
    {
        ContractMetric? channelMetric = null;
        ContractMetric? typeMetric;
        if (entry.Sent)
        {
            sentGlobalMetric.AddMessageRecord(entry.MessageSize, entry.Duration);
            if (!sentTypeMetrics.TryGetValue(entry.Type, out typeMetric))
            {
                typeMetric = new();
                sentTypeMetrics.TryAdd(entry.Type, typeMetric);
            }
            if (!string.IsNullOrWhiteSpace(entry.Channel) && !sentChannelMetrics.TryGetValue(entry.Channel, out channelMetric))
            {
                channelMetric = new();
                sentChannelMetrics.TryAdd(entry.Channel, channelMetric);
            }
        }
        else
        {
            receivedGlobalMetric.AddMessageRecord(entry.MessageSize, entry.Duration);
            if (!receivedTypeMetrics.TryGetValue(entry.Type, out typeMetric))
            {
                typeMetric = new();
                receivedTypeMetrics.TryAdd(entry.Type, typeMetric);
            }
            if (!string.IsNullOrWhiteSpace(entry.Channel) && !receivedChannelMetrics.TryGetValue(entry.Channel, out channelMetric))
            {
                channelMetric = new();
                receivedChannelMetrics.TryAdd(entry.Channel, channelMetric);
            }
        }
        typeMetric?.AddMessageRecord(entry.MessageSize, entry.Duration);
        channelMetric?.AddMessageRecord(entry.MessageSize, entry.Duration);
    }

    public IContractMetric GetSnapshot(bool sent)
        => (sent ? sentGlobalMetric.ToReadonly() : receivedGlobalMetric.ToReadonly());

    public IContractMetric? GetSnapshot(Type messageType, bool sent)
    {
        IContractMetric? result = null;
        if (sent && sentTypeMetrics.TryGetValue(messageType, out var sentValue))
            result = sentValue.ToReadonly();
        else if (!sent && receivedTypeMetrics.TryGetValue(messageType, out var receivedValue))
            result = receivedValue.ToReadonly();
        return result;
    }

    public IContractMetric? GetSnapshot(string channel, bool sent)
    {
        IContractMetric? result = null;
        if (sent && sentChannelMetrics.TryGetValue(channel, out var sentValue))
            result = sentValue.ToReadonly();
        else if (!sent && receivedChannelMetrics.TryGetValue(channel, out var receivedValue))
            result = receivedValue.ToReadonly();
        return result;
    }
}
