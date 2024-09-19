using MQContract.Interfaces;

namespace MQContract.Middleware.Metrics
{
    internal record ReadonlyContractMetric(ulong Messages,ulong MessageBytes,ulong MessageBytesAverage, ulong MessageBytesMin, ulong MessageBytesMax,
        TimeSpan MessageConversionDuration,TimeSpan MessageConversionAverage,TimeSpan MessageConversionMin, TimeSpan MessageConversionMax
        )
        : IContractMetric
    {
        public ReadonlyContractMetric(ContractMetric metric)
            : this(metric.Messages, metric.MessageBytes, metric.MessageBytesAverage, metric.MessageBytesMin, metric.MessageBytesMax,
                  metric.MessageConversionDuration, metric.MessageConversionAverage, metric.MessageConversionMin, metric.MessageConversionMax)
        { }
    }
}
