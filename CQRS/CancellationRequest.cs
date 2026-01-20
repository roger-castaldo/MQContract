namespace MQContract.CQRS
{
    internal record CancellationRequest(Guid CorrelationId, Guid MessageId)
    { }
}
