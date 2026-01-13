using MQContract.Defaults;
using MQContract.Messages;

namespace MQContract
{
    internal partial class MessageContext
    {
        private readonly List<MQContractMessageContext> contexts = [new DefaultMessageContext()];

        internal void RegisterContext(MQContractMessageContext messageContext)
            => contexts.Add(messageContext);

        internal delegate ValueTask<ServiceMessage> delProduceServiceMessage<TMessage>(TMessage message);
    }
}
