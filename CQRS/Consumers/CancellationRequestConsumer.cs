using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;

namespace MQContract.CQRS.Consumers
{
    internal class CancellationRequestConsumer(SemaphoreSlim lockSlim, List<InvocationInstance> invocationInstances) : IPubSubAsyncConsumer<CancellationRequest>
    {
        void IBaseConsumer.ErrorRecieved(Exception error)
        {}

        async ValueTask IPubSubAsyncConsumer<CancellationRequest>.MessageReceivedAsync(IReceivedMessage<CancellationRequest> message)
        {
            await lockSlim.WaitAsync();
            foreach (var instance in invocationInstances.Where(inst => inst.IsMatch(message.Message)).ToArray())
            {
                try
                {
                    if (!instance.CancellationTokenSource.IsCancellationRequested)
                        instance.CancellationTokenSource.Cancel();
                }
                catch
                {
                    //no exception catch needed
                }
                invocationInstances.Remove(instance);
            }
            lockSlim.Release();
        }
    }
}
