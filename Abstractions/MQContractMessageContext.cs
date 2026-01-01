using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Messages;

namespace MQContract
{
    public abstract class MQContractMessageContext
    {
        public readonly record struct MessageTypeDefinition(string? Channel, string TypeName, Version TypeVersion, string? ResponseChannel, TimeSpan? ResponseTimeout, Type? ResponseType);

        public virtual object? TryGetMessageEncoder<TMessage>(IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
            => null;

        public virtual object? TryGetMessageEncoder(string messageID, IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
            => null;

        public virtual MessageTypeDefinition? TryGetMessageType(Type messageType)
            => null;

        public virtual Func<IEncodedMessage, ValueTask<object?>> TryGetMessageConverter<TMessage>(string messageID, Func<IEncodedMessage, ValueTask<object?>> messageDecode, IServiceProvider? serviceProvider)
            => null;
    }
}
