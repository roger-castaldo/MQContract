using Apache.NMS;
using Apache.NMS.Util;
using MQContract.Interfaces.Service;

namespace MQContract.ActiveMQ.Subscriptions
{
    internal class SubscriptionBase(Action<IMessage> messageReceived,Action<Exception> errorReceived, ConsumerInstance consumer) : IServiceSubscription
    {
        private bool disposedValue;
        protected readonly CancellationTokenSource cancelToken = new();
        
        internal async ValueTask StartAsync()
        {
            _=Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var msg = await consumer.ReceiveAsync();
                        if (msg!=null)  
                            messageReceived(msg);
                    }
                    catch (Exception ex)
                    {
                        errorReceived(ex);
                    }
                }
            });
        }

        public async ValueTask EndAsync()
        {
            if (!cancelToken.IsCancellationRequested)
                await cancelToken.CancelAsync();
            if (consumer!=null)
                await consumer.CloseAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                await EndAsync();
                consumer?.Dispose();
            }
        }
    }
}
