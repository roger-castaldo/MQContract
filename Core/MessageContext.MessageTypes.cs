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
                var def = contexts.FirstOrDefault(context => context.IsMessageCodeGenerated(messageType))?
                    .TryGetMessageType(messageType);
                if (def!=null)
                    messageDefinition = def.Value;
                else
                {
                    var queryAttribute = messageType.GetCustomAttribute<QueryMessageAttribute>();
                    var messageAttribute = (queryAttribute==null ? messageType.GetCustomAttribute<MessageAttribute>() : (MessageAttribute)queryAttribute);
                    messageDefinition = new(messageAttribute?.Channel, GetMessageName(messageType, messageAttribute), messageAttribute?.TypeVersion??new("0.0.0.0"), queryAttribute?.ResponseChannel, queryAttribute?.ResponseTimeout, queryAttribute?.ResponseType);
                }
                cache.TryAdd(messageType, messageDefinition);
            }
            return messageDefinition;
        }

        private static string GetMessageName(Type messageType, MessageAttribute? messageAttribute)
        {
            var name = messageAttribute?.TypeName??messageType.Name;
            if (messageAttribute==null && (!(messageType.FullName?.EndsWith(name, StringComparison.InvariantCultureIgnoreCase)??false) || string.IsNullOrWhiteSpace(messageType.Name)))
            {
                name = messageType.FullName??messageType.Name;
                if (!string.IsNullOrWhiteSpace(messageType.Name) && name.Contains(messageType.Name, StringComparison.InvariantCultureIgnoreCase))
                    name = name.Substring(name.IndexOf(messageType.Name, StringComparison.InvariantCultureIgnoreCase));
                name = FixInternalBrackets(name);
            }
            return name;
        }

        private static string FixInternalBrackets(string name)
        {
            if (name.Contains('<'))
            {
                var preBracket = name.Substring(0, name.IndexOf("<")+1);
                var betweenBrackets = name.Substring(preBracket.Length, name.Length-1-preBracket.Length);
                return $"{preBracket}{FixInternalBrackets(betweenBrackets)}>";
            }
            else if (name.Contains(","))
            {
                var splt = name.Split(',');
                for (var x = 0; x<splt.Length; x++)
                    splt[x]=FixInternalBrackets(splt[x]);
                return string.Join(",", splt);
            }
            else if (name.Contains('.'))
                return name.Substring(name.LastIndexOf('.')+1);
            return name;
        }
    }
}
