using MQContract.Attributes;
using System.Collections.Concurrent;
using System.Reflection;

namespace MQContract
{
    internal partial class MessageContext
    {
        internal readonly ConcurrentDictionary<Type, MQContract.MQContractMessageContext.MessageTypeDefinition> cache = new()
        {
            [typeof(ushort)] = new(null, typeof(ushort).Name, new("0.0.0.0"), null, null, null),
            [typeof(ushort[])] = new(null, typeof(ushort[]).Name, new("0.0.0.0"), null, null, null),
            [typeof(IEnumerable<ushort>)] = new(null, typeof(IEnumerable<ushort>).Name, new("0.0.0.0"), null, null, null),
            [typeof(string)] = new(null, typeof(string).Name, new("0.0.0.0"), null, null, null),
            [typeof(char)] = new(null, typeof(char).Name, new("0.0.0.0"), null, null, null),
            [typeof(short)] = new(null, typeof(short).Name, new("0.0.0.0"), null, null, null),
            [typeof(short[])] = new(null, typeof(short[]).Name, new("0.0.0.0"), null, null, null),
            [typeof(IEnumerable<short>)] = new(null, typeof(IEnumerable<short>).Name, new("0.0.0.0"), null, null, null),
            [typeof(long)] = new(null, typeof(long).Name, new("0.0.0.0"), null, null, null),
            [typeof(long[])] = new(null, typeof(long[]).Name, new("0.0.0.0"), null, null, null),
            [typeof(IEnumerable<long>)] = new(null, typeof(IEnumerable<long>).Name, new("0.0.0.0"), null, null, null),
            [typeof(ulong)] = new(null, typeof(ulong).Name, new("0.0.0.0"), null, null, null),
            [typeof(ulong[])] = new(null, typeof(ulong[]).Name, new("0.0.0.0"), null, null, null),
            [typeof(IEnumerable<ulong>)] = new(null, typeof(IEnumerable<ulong>).Name, new("0.0.0.0"), null, null, null),
            [typeof(uint)] = new(null, typeof(uint).Name, new("0.0.0.0"), null, null, null),
            [typeof(uint[])] = new(null, typeof(uint[]).Name, new("0.0.0.0"), null, null, null),
            [typeof(IEnumerable<uint>)] = new(null, typeof(IEnumerable<uint>).Name, new("0.0.0.0"), null, null, null),
            [typeof(int)] = new(null, typeof(int).Name, new("0.0.0.0"), null, null, null),
            [typeof(int[])] = new(null, typeof(int[]).Name, new("0.0.0.0"), null, null, null),
            [typeof(IEnumerable<int>)] = new(null, typeof(IEnumerable<int>).Name, new("0.0.0.0"), null, null, null),
            [typeof(Half)] = new(null, typeof(Half).Name, new("0.0.0.0"), null, null, null),
            [typeof(Half[])] = new(null, typeof(Half[]).Name, new("0.0.0.0"), null, null, null),
            [typeof(IEnumerable<Half>)] = new(null, typeof(IEnumerable<Half>).Name, new("0.0.0.0"), null, null, null),
            [typeof(float)] = new(null, typeof(float).Name, new("0.0.0.0"), null, null, null),
            [typeof(float[])] = new(null, typeof(float[]).Name, new("0.0.0.0"), null, null, null),
            [typeof(IEnumerable<float>)] = new(null, typeof(IEnumerable<float>).Name, new("0.0.0.0"), null, null, null),
            [typeof(double)] = new(null, typeof(double).Name, new("0.0.0.0"), null, null, null),
            [typeof(double[])] = new(null, typeof(double[]).Name, new("0.0.0.0"), null, null, null),
            [typeof(IEnumerable<double>)] = new(null, typeof(IEnumerable<double>).Name, new("0.0.0.0"), null, null, null),
            [typeof(decimal)] = new(null, typeof(decimal).Name, new("0.0.0.0"), null, null, null),
            [typeof(decimal[])] = new(null, typeof(decimal[]).Name, new("0.0.0.0"), null, null, null),
            [typeof(IEnumerable<decimal>)] = new(null, typeof(IEnumerable<decimal>).Name, new("0.0.0.0"), null, null, null),
            [typeof(byte)] = new(null, typeof(byte).Name, new("0.0.0.0"), null, null, null),
            [typeof(byte[])] = new(null, typeof(byte[]).Name, new("0.0.0.0"), null, null, null),
            [typeof(bool)] = new(null, typeof(bool).Name, new("0.0.0.0"), null, null, null),
            [typeof(bool[])] = new(null, typeof(bool[]).Name, new("0.0.0.0"), null, null, null),
            [typeof(IEnumerable<bool>)] = new(null, typeof(IEnumerable<bool>).Name, new("0.0.0.0"), null, null, null)
        };

        public string MessageTypeName<TMessage>()
            => MessageTypeName(typeof(TMessage));

        public string MessageTypeName(Type messageType)
            => GetMessageAttribute(messageType).TypeName;

        public string MessageVersionString<TMessage>()
            => MessageVersionString(typeof(TMessage));

        public string MessageVersionString(Type messageType)
            => GetMessageAttribute(messageType).TypeVersion.ToString();

        public string? MessageChannel<TMessage>()
            => GetMessageAttribute(typeof(TMessage)).Channel;

        public TimeSpan? QueryResponseTimeout<TQuery>()
            => GetMessageAttribute(typeof(TQuery)).ResponseTimeout;

        public string? QueryResponseChannel<TQuery>()
            => GetMessageAttribute(typeof(TQuery)).ResponseChannel;

        public Type? QueryResponseType<TQuery>()
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
