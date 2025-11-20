using MQContract.Attributes;
using MQContract.Messages;
using System.Collections.Concurrent;
using System.Reflection;

namespace MQContract
{
    internal static class Utility
    {
        private static readonly ConcurrentDictionary<(Type objectType, Type attributeType), Attribute?> attributeCache = [];

        internal static TAttribute? GetCustomAttribute<TAttribute>(Type type,bool inherit = false)
            where TAttribute : Attribute
        {
            if (!attributeCache.TryGetValue((type, typeof(TAttribute)),out var att))
            {
                att = type.GetCustomAttribute<TAttribute>(inherit);
                attributeCache.TryAdd((type, typeof(TAttribute)), att);
            }
            return (TAttribute?)att;
        }

        internal static TAttribute? GetCustomAttribute<TAttributeHolder, TAttribute>(bool inherit = false)
            where TAttribute : Attribute
            => GetCustomAttribute<TAttribute>(typeof(TAttributeHolder), inherit);

        internal static string MessageTypeName<TMessage>()
            => MessageTypeName(typeof(TMessage));

        internal static string MessageTypeName(Type messageType)
            => GetCustomAttribute<MessageAttribute>(messageType)?.TypeName??TypeName(messageType);

        internal static string TypeName<TMessage>()
            => TypeName(typeof(TMessage));

        internal static string TypeName(Type type)
        {
            var result = type.Name;
            if (result.Contains('`'))
                result=result[..result.IndexOf('`')];
            return result;
        }

        internal static string MessageVersionString<TMessage>()
            => MessageVersionString(typeof(TMessage));

        internal static string MessageVersionString(Type messageType)
            => GetCustomAttribute<MessageAttribute>(messageType)?.TypeVersion.ToString()??"0.0.0.0";

        internal static async ValueTask<object?> InvokeMethodAsync(MethodInfo method, object container, object?[]? parameters)
        {
            var valueTask = method.Invoke(container, parameters)!;
            await (Task)valueTask.GetType().GetMethod(nameof(ValueTask.AsTask))!.Invoke(valueTask, null)!;
            return valueTask.GetType().GetProperty(nameof(ValueTask<object>.Result))!.GetValue(valueTask);
        }

        internal async static ValueTask<string> GetChannelAsync<TMessage>(Func<string, ValueTask<string>> mapChannel, string? channel = null)
            => await mapChannel(channel??GetCustomAttribute<TMessage, MessageAttribute>()?.Channel??throw new MessageChannelNullException());

        internal static string GetChannel<TMessage>(Func<string, ValueTask<string>> mapChannel, string? channel = null)
        {
            var chan = channel??GetCustomAttribute<TMessage, MessageAttribute>()?.Channel??throw new MessageChannelNullException();
            var tsk = mapChannel(chan).AsTask();
            tsk.Wait();
            return tsk.Result;
        }

        internal static QueryResult<object>? ConvertResultFromObject(object? obj)
        {
            if (obj == null) return null;
            var type = obj.GetType();
            if (type.IsGenericType && Equals(type.GetGenericTypeDefinition(), typeof(QueryResult<>)))
            {
                return new(
                    (string)type.GetProperty(nameof(QueryResult<object>.ID))!.GetValue(obj)!,
                    (MessageHeader)type.GetProperty(nameof(QueryResult<object>.Header))!.GetValue(obj)!,
                    type.GetProperty(nameof(QueryResult<object>.Result))!.GetValue(obj),
                    (ErrorMessage?)type.GetProperty(nameof(QueryResult<object>.Error))!.GetValue(obj)
                );
            }
            throw new InvalidCastException();
        }
    }
}
