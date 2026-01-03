using MQContract.Defaults;

namespace MQContract
{
    internal partial class MessageContext
    {
        private readonly List<MQContractMessageContext> contexts = [new DefaultMessageContext()];

        internal void RegisterContext(MQContractMessageContext messageContext)
            => contexts.Add(messageContext);
    }
}
