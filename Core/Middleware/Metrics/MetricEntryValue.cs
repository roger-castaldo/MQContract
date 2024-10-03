namespace MQContract.Middleware.Metrics
{
    internal record MetricEntryValue(Type Type, string? Channel,bool Sent, int MessageSize, TimeSpan Duration)
    { }
}
