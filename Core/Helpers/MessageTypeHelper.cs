using MQContract.Attributes;
using System.Collections.Concurrent;
using System.Reflection;

namespace MQContract.Helpers
{
    internal static partial class MessageTypeHelper
    {
        internal readonly record struct MessageTypeDefinition(string? Channel, string TypeName, Version TypeVersion);

        internal static readonly ConcurrentDictionary<Type, MessageTypeDefinition> cache = new()
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
        internal static string MessageTypeName<TMessage>()
            => MessageTypeName(typeof(TMessage));

        internal static string MessageTypeName(Type messageType)
            => GetMessageAttribute(messageType).TypeName;

        internal static string MessageVersionString<TMessage>()
            => MessageVersionString(typeof(TMessage));

        internal static string MessageVersionString(Type messageType)
            => GetMessageAttribute(messageType).TypeVersion.ToString();

        internal static string? MessageChannel<TMessage>()
            => MessageChannel(typeof(TMessage));

        internal static string? MessageChannel(Type messageType)
            => GetMessageAttribute(messageType).Channel;

        private static MessageTypeDefinition GetMessageAttribute(Type messageType)
        {
            if (!cache.TryGetValue(messageType, out var messageDefinition))
            {
                var (channel, name, version) = TryGetType(messageType);
                if (name!=null)
                    messageDefinition = new(channel, name!, version!);
                else
                {
                    var messageAttribute = messageType.GetCustomAttribute<MessageAttribute>();
                    name = messageAttribute?.TypeName;
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
