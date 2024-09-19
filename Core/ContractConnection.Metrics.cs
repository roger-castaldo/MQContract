using MQContract.Interfaces;
using MQContract.Middleware;

namespace MQContract
{
    public sealed partial class ContractConnection
    {
        void IContractConnection.AddMetrics(bool useMeter, bool useInternal)
        {
            lock (middleware)
            {
                middleware.Insert(0, new MetricsMiddleware(useMeter, useInternal));
            }
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
        IContractMetric? IContractConnection.GetSnapshot(string channel, bool sent)
            => MetricsMiddleware?.GetSnapshot(channel, sent);
    }
}
