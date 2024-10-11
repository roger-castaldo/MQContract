using MQContract.Interfaces;

namespace MQContract.Middleware.Metrics
{
    internal record ContractMetric : IContractMetric
    {
        public ulong Messages { get; private set; } = 0;

        public ulong MessageBytes { get; private set; } = 0;

        public ulong MessageBytesAverage => (Messages==0 ? 0 : MessageBytes / Messages);

        public ulong MessageBytesMin { get; private set; } = ulong.MaxValue;

        public ulong MessageBytesMax { get; private set; } = ulong.MinValue;

        public TimeSpan MessageConversionDuration { get; private set; } = TimeSpan.Zero;

        public TimeSpan MessageConversionAverage => (Messages==0 ? TimeSpan.Zero : MessageConversionDuration / Messages);

        public TimeSpan MessageConversionMin { get; private set; } = TimeSpan.MaxValue;

        public TimeSpan MessageConversionMax { get; private set; } = TimeSpan.MinValue;

        public void AddMessageRecord(int messageSize, TimeSpan encodingDuration)
        {
            Messages++;
            MessageBytes += (ulong)messageSize;
            MessageBytesMin = Math.Min(MessageBytesMin, (ulong)messageSize);
            MessageBytesMax = Math.Max(MessageBytesMax, (ulong)messageSize);
            MessageConversionDuration += encodingDuration;
            MessageConversionMin = TimeSpan.FromTicks(Math.Min(MessageConversionMin.Ticks, encodingDuration.Ticks));
            MessageConversionMax = TimeSpan.FromTicks(Math.Max(MessageConversionMax.Ticks, encodingDuration.Ticks));
        }
    }
}
