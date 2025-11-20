using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;
using System.Collections.Concurrent;

namespace MQContract.CQRS.Consumers
{
    internal class CancellationRequestConsumer(ConcurrentDictionary<(Guid messageId, Guid correlationId, Guid? causationId), CancellationTokenSource> invocationInstances) : IPubSubAsyncConsumer<CancellationRequest>
    {
        void IBaseConsumer.ErrorRecieved(Exception error)
        {}

        async ValueTask IPubSubAsyncConsumer<CancellationRequest>.MessageReceivedAsync(IReceivedMessage<CancellationRequest> message)
        {
            var keys = invocationInstances.Keys.Where(key => Equals(message.Message.CorrelationId, key.correlationId)
            && (Equals(message.Message.MessageId,key.messageId) || Equals(message.Message.MessageId,key.causationId))).ToArray();
            foreach(var key in keys)
            {
                if (invocationInstances.TryRemove(key,out var cancellationTokenSource))
                {
                    try
                    {
                        if (!cancellationTokenSource.IsCancellationRequested)
                            await cancellationTokenSource.CancelAsync();
                    }
                    catch
                    {
                        //no exception catch needed
                    }
                }
            }
        }
    }
}
