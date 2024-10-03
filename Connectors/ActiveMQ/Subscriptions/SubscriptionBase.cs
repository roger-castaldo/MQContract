using Apache.NMS;
using Apache.NMS.Util;
using MQContract.Interfaces.Service;

namespace MQContract.ActiveMQ.Subscriptions
{
    internal class SubscriptionBase(Action<IMessage> messageReceived,Action<Exception> errorReceived,ISession session, string channel,string group) : IServiceSubscription
    {
        private bool disposedValue;
        private IMessageConsumer? consumer;
        protected readonly CancellationTokenSource cancelToken = new();
        protected string Channel => channel;
        
        internal async ValueTask StartAsync()
        {
            consumer = await session.CreateSharedConsumerAsync(SessionUtil.GetTopic(session, channel),group);
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
