namespace MQContract.CQRS
{
    internal sealed record InvocationInstance(Guid MessageId, Guid CorrelationId, Guid? CausationId, CancellationTokenSource CancellationTokenSource)
    {
        public bool IsMatch(CancellationRequest request)
            => Equals(CorrelationId, request.CorrelationId)
            && (Equals(MessageId, request.MessageId) || Equals(CausationId, request.MessageId));
    }
}
