namespace MQContract
{
    internal partial class MessageContext
    {
        private readonly List<MQContractMessageContext> contexts = [];

        internal void RegisterContext(MQContractMessageContext messageContext)
            => contexts.Add(messageContext);
    }
}
