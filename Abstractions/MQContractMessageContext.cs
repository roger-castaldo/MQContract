using MQContract.Interfaces.Encoding;

namespace MQContract
{
    public abstract class MQContractMessageContext
    {
        public readonly record struct MessageTypeDefinition(string? Channel, string TypeName, Version TypeVersion);

        public virtual object? TryGetMessageEncoder<TMessage>(IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
            => null;

        public virtual MessageTypeDefinition? TryGetMessageType(Type messageType)
            => null;
    }
}
