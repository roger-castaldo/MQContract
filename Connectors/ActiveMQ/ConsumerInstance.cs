using Apache.NMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.ActiveMQ
{
    internal class ConsumerInstance(string channel,string group, IMessageConsumer messageConsumer,Action cleanup)
    {
        public string Channel => channel;
        public string Group =>group;

        private int listenerCount = 1;

        public void AddListener() => listenerCount++;

        public Task<IMessage> ReceiveAsync() => messageConsumer.ReceiveAsync();

        public async Task CloseAsync()
        {
            listenerCount--;
            if (listenerCount == 0)
            {
                await messageConsumer.CloseAsync();
                cleanup();
            }
        }

        public void Dispose()
        {
            messageConsumer.Dispose();
        }
    }
}
