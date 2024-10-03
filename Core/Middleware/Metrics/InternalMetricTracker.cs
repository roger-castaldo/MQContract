namespace MQContract.Middleware.Metrics
{
    internal class InternalMetricTracker
    {
        private readonly SemaphoreSlim semDataLock = new(1, 1);
        private readonly ContractMetric sentGlobalMetric = new();
        private readonly ContractMetric receivedGlobalMetric = new();
        private readonly Dictionary<Type, ContractMetric> sentTypeMetrics = [];
        private readonly Dictionary<Type,ContractMetric> receivedTypeMetrics = [];
        private readonly Dictionary<string, ContractMetric> sentChannelMetrics = [];
        private readonly Dictionary<string, ContractMetric> receivedChannelMetrics = [];
        
        public void AppendEntry(MetricEntryValue entry) {
            semDataLock.Wait();
            ContractMetric? channelMetric = null;
            ContractMetric? typeMetric = null;
            if (entry.Sent)
            {
                sentGlobalMetric.AddMessageRecord(entry.MessageSize, entry.Duration);
                if (!sentTypeMetrics.TryGetValue(entry.Type, out typeMetric))
                {
                    typeMetric = new();
                    sentTypeMetrics.Add(entry.Type, typeMetric);
                }
                if (!string.IsNullOrWhiteSpace(entry.Channel) && !sentChannelMetrics.TryGetValue(entry.Channel, out channelMetric))
                {
                    channelMetric = new();
                    sentChannelMetrics.Add(entry.Channel, channelMetric);
                }
            }
            else
            {
                receivedGlobalMetric.AddMessageRecord(entry.MessageSize, entry.Duration);
                if (!receivedTypeMetrics.TryGetValue(entry.Type, out typeMetric))
                {
                    typeMetric = new();
                    receivedTypeMetrics.Add(entry.Type, typeMetric);
                }
                if (!string.IsNullOrWhiteSpace(entry.Channel) && !receivedChannelMetrics.TryGetValue(entry.Channel, out channelMetric))
                {
                    channelMetric = new();
                    receivedChannelMetrics.Add(entry.Channel, channelMetric);
                }
            }
            typeMetric?.AddMessageRecord(entry.MessageSize,entry.Duration);
            channelMetric?.AddMessageRecord(entry.MessageSize, entry.Duration);
            semDataLock.Release();
        }

        public ReadonlyContractMetric GetSnapshot(bool sent)
        {
            semDataLock.Wait();
            var result = new ReadonlyContractMetric((sent ? sentGlobalMetric : receivedGlobalMetric));
            semDataLock.Release();
            return result;
        }

        public ReadonlyContractMetric? GetSnapshot(Type messageType,bool sent)
        {
            ReadonlyContractMetric? result = null;
            semDataLock.Wait();
            if (sent && sentTypeMetrics.TryGetValue(messageType, out var sentValue))
                result = new ReadonlyContractMetric(sentValue!);
            else if (!sent && receivedTypeMetrics.TryGetValue(messageType, out var receivedValue))
                result = new ReadonlyContractMetric(receivedValue!);
            semDataLock.Release();
            return result;
        }

        public ReadonlyContractMetric? GetSnapshot(string channel, bool sent)
        {
            ReadonlyContractMetric? result = null;
            semDataLock.Wait();
            if (sent && sentChannelMetrics.TryGetValue(channel, out var sentValue))
                result = new ReadonlyContractMetric(sentValue!);
            else if (!sent && receivedChannelMetrics.TryGetValue(channel, out var receivedValue))
                result = new ReadonlyContractMetric(receivedValue!);
            semDataLock.Release();
            return result;
        }
    }
}
