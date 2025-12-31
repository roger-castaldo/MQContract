using MQContract.Attributes;
using System.Collections.Concurrent;
using System.Reflection;

namespace MQContract
{
    internal partial class MessageContext
    {
        internal readonly ConcurrentDictionary<Type, MQContract.MQContractMessageContext.MessageTypeDefinition> cache = new()
        {
            [typeof(ushort)] = new(null, typeof(ushort).Name, new("0.0.0.0")),
            [typeof(ushort[])] = new(null, typeof(ushort[]).Name, new("0.0.0.0")),
            [typeof(IEnumerable<ushort>)] = new(null, typeof(IEnumerable<ushort>).Name, new("0.0.0.0")),
            [typeof(string)] = new(null, typeof(string).Name, new("0.0.0.0")),
            [typeof(char)] = new(null, typeof(char).Name, new("0.0.0.0")),
            [typeof(short)] = new(null, typeof(short).Name, new("0.0.0.0")),
            [typeof(short[])] = new(null, typeof(short[]).Name, new("0.0.0.0")),
            [typeof(IEnumerable<short>)] = new(null, typeof(IEnumerable<short>).Name, new("0.0.0.0")),
            [typeof(long)] = new(null, typeof(long).Name, new("0.0.0.0")),
            [typeof(long[])] = new(null, typeof(long[]).Name, new("0.0.0.0")),
            [typeof(IEnumerable<long>)] = new(null, typeof(IEnumerable<long>).Name, new("0.0.0.0")),
            [typeof(ulong)] = new(null, typeof(ulong).Name, new("0.0.0.0")),
            [typeof(ulong[])] = new(null, typeof(ulong[]).Name, new("0.0.0.0")),
            [typeof(IEnumerable<ulong>)] = new(null, typeof(IEnumerable<ulong>).Name, new("0.0.0.0")),
            [typeof(uint)] = new(null, typeof(uint).Name, new("0.0.0.0")),
            [typeof(uint[])] = new(null, typeof(uint[]).Name, new("0.0.0.0")),
            [typeof(IEnumerable<uint>)] = new(null, typeof(IEnumerable<uint>).Name, new("0.0.0.0")),
            [typeof(int)] = new(null, typeof(int).Name, new("0.0.0.0")),
            [typeof(int[])] = new(null, typeof(int[]).Name, new("0.0.0.0")),
            [typeof(IEnumerable<int>)] = new(null, typeof(IEnumerable<int>).Name, new("0.0.0.0")),
            [typeof(Half)] = new(null, typeof(Half).Name, new("0.0.0.0")),
            [typeof(Half[])] = new(null, typeof(Half[]).Name, new("0.0.0.0")),
            [typeof(IEnumerable<Half>)] = new(null, typeof(IEnumerable<Half>).Name, new("0.0.0.0")),
            [typeof(float)] = new(null, typeof(float).Name, new("0.0.0.0")),
            [typeof(float[])] = new(null, typeof(float[]).Name, new("0.0.0.0")),
            [typeof(IEnumerable<float>)] = new(null, typeof(IEnumerable<float>).Name, new("0.0.0.0")),
            [typeof(double)] = new(null, typeof(double).Name, new("0.0.0.0")),
            [typeof(double[])] = new(null, typeof(double[]).Name, new("0.0.0.0")),
            [typeof(IEnumerable<double>)] = new(null, typeof(IEnumerable<double>).Name, new("0.0.0.0")),
            [typeof(decimal)] = new(null, typeof(decimal).Name, new("0.0.0.0")),
            [typeof(decimal[])] = new(null, typeof(decimal[]).Name, new("0.0.0.0")),
            [typeof(IEnumerable<decimal>)] = new(null, typeof(IEnumerable<decimal>).Name, new("0.0.0.0")),
            [typeof(byte)] = new(null, typeof(byte).Name, new("0.0.0.0")),
            [typeof(byte[])] = new(null, typeof(byte[]).Name, new("0.0.0.0")),
            [typeof(bool)] = new(null, typeof(bool).Name, new("0.0.0.0")),
            [typeof(bool[])] = new(null, typeof(bool[]).Name, new("0.0.0.0")),
            [typeof(IEnumerable<bool>)] = new(null, typeof(IEnumerable<bool>).Name, new("0.0.0.0"))
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
                    var messageAttribute = messageType.GetCustomAttribute<MessageAttribute>();
                    var name = messageAttribute?.TypeName;
                    if (name==null)
                    {
                        name = messageType.Name;
                        if (name.Contains('`'))
                            name=name[..name.IndexOf('`')];
                    }
                    messageDefinition = new(messageAttribute?.Channel, name, messageAttribute?.TypeVersion??new("0.0.0.0"));
                }
                cache.TryAdd(messageType, messageDefinition);
            }
            return messageDefinition;
        }
    }
}
