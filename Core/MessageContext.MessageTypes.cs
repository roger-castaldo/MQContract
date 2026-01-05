using MQContract.Attributes;
using System.Collections.Concurrent;
using System.Reflection;

namespace MQContract
{
    internal partial class MessageContext
    {
        internal readonly ConcurrentDictionary<Type, MQContract.MQContractMessageContext.MessageTypeDefinition> cache = [];

        public string MessageTypeName<TMessage>()
            => MessageTypeName(typeof(TMessage));

        public string MessageTypeName(Type messageType)
            => GetMessageAttribute(messageType).TypeName;

        public string MessageVersionString<TMessage>()
            => MessageVersionString(typeof(TMessage));

        public string MessageVersionString(Type messageType)
            => GetMessageAttribute(messageType).TypeVersion.ToString();

        public string MessageID<TMessage>()
            => MessageID(typeof(TMessage));

        public string MessageID(Type messageType)
            => $"{MessageTypeName(messageType)}-{MessageVersionString(messageType)}";

        public string? MessageChannel<TMessage>()
            => GetMessageAttribute(typeof(TMessage)).Channel;

        public TimeSpan? QueryResponseTimeout<TQuery>()
            => GetMessageAttribute(typeof(TQuery)).ResponseTimeout;

        public string? QueryResponseChannel<TQuery>()
            => GetMessageAttribute(typeof(TQuery)).ResponseChannel;

        private Type? QueryResponseType<TQuery>()
            => GetMessageAttribute(typeof(TQuery)).ResponseType;

        private MQContract.MQContractMessageContext.MessageTypeDefinition GetMessageAttribute(Type messageType)
        {
            if (!cache.TryGetValue(messageType, out var messageDefinition))
            {
                var found = false;
                foreach(var context in contexts)
                {
                    var def = context.TryGetMessageType(messageType);
                    if (def!=null)
                    {
                        messageDefinition = def.Value;
                        found=true;
                        break;
                    }
                }
                if (!found)
                {
                    var queryAttribute = messageType.GetCustomAttribute<QueryMessageAttribute>();
                    var messageAttribute = (queryAttribute==null ? messageType.GetCustomAttribute<MessageAttribute>() : (MessageAttribute)queryAttribute);
                    var name = messageAttribute?.TypeName;
                    if (name==null)
                    {
                        name = messageType.Name;
                        if (name.Contains('`'))
                            name=name[..name.IndexOf('`')];
                    }
                    messageDefinition = new(messageAttribute?.Channel, name, messageAttribute?.TypeVersion??new("0.0.0.0"), queryAttribute?.ResponseChannel, queryAttribute?.ResponseTimeout, queryAttribute?.ResponseType);
                }
                cache.TryAdd(messageType, messageDefinition);
            }
            return messageDefinition;
        }
    }
}
