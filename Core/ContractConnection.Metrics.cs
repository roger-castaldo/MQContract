using MQContract.Interfaces;
using MQContract.Middleware;
using System.Diagnostics.Metrics;

namespace MQContract
{
    public sealed partial class ContractConnection
    {
        IContractConnection IContractConnection.AddMetrics(Meter? meter, bool useInternal)
        {
            lock (middleware)
            {
                middleware.Insert(0, new MetricsMiddleware(meter, useInternal));
            }
            return this;
        }

        private MetricsMiddleware? MetricsMiddleware
        {
            get
            {
                MetricsMiddleware? metricsMiddleware;
                lock (middleware)
                {
                    metricsMiddleware = middleware.OfType<MetricsMiddleware>().FirstOrDefault();
                }
                return metricsMiddleware;
            }
        }

        IContractMetric? IContractConnection.GetSnapshot(bool sent)
            => MetricsMiddleware?.GetSnapshot(sent);
        IContractMetric? IContractConnection.GetSnapshot(Type messageType, bool sent)
            => MetricsMiddleware?.GetSnapshot(messageType, sent);
        IContractMetric? IContractConnection.GetSnapshot<T>(bool sent)
            => MetricsMiddleware?.GetSnapshot(typeof(T), sent);
        IContractMetric? IContractConnection.GetSnapshot(string channel, bool sent)
            => MetricsMiddleware?.GetSnapshot(channel, sent);
    }
}
