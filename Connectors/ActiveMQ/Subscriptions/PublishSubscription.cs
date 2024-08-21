using Apache.NMS;
using Apache.NMS.Util;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.ActiveMQ.Subscriptions
{
    internal class PublishSubscription(ISession session, string channel, Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved) : IServiceSubscription
    {
        private bool disposedValue;
        private IMessageConsumer? consumer;
        
        internal async ValueTask StartAsync(CancellationToken cancellationToken)
        {
            consumer = await session.CreateConsumerAsync(SessionUtil.GetTopic(session, channel));
            consumer.Listener+=ConsumeMessage;
            cancellationToken.Register(async () => await EndAsync());
        }

        private void ConsumeMessage(IMessage message)
        {
            try
            {
                messageRecieved(Connection.ProduceMessage(channel, message));
            }catch(Exception e)
            {
                errorRecieved(e);
            }
        }

        public async ValueTask EndAsync()
        {
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
