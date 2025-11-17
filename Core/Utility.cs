using MQContract.Attributes;
using MQContract.Messages;
using System.Reflection;

namespace MQContract
{
    internal static class Utility
    {
        internal static string MessageTypeName<T>()
            => MessageTypeName(typeof(T));

        internal static string MessageTypeName(Type messageType)
            => messageType.GetCustomAttribute<MessageAttribute>()?.TypeName??TypeName(messageType);

        internal static string TypeName<T>()
            => TypeName(typeof(T));

        internal static string TypeName(Type type)
        {
            var result = type.Name;
            if (result.Contains('`'))
                result=result[..result.IndexOf('`')];
            return result;
        }

        internal static string MessageVersionString<T>()
            => MessageVersionString(typeof(T));

        internal static string MessageVersionString(Type messageType)
            => messageType.GetCustomAttribute<MessageAttribute>()?.TypeVersion.ToString()??"0.0.0.0";

        internal static async ValueTask<object?> InvokeMethodAsync(MethodInfo method, object container, object?[]? parameters)
        {
            var valueTask = method.Invoke(container, parameters)!;
            await (Task)valueTask.GetType().GetMethod(nameof(ValueTask.AsTask))!.Invoke(valueTask, null)!;
            return valueTask.GetType().GetProperty(nameof(ValueTask<object>.Result))!.GetValue(valueTask);
        }

        internal async static ValueTask<string> GetChannelAsync<T>(Func<string, ValueTask<string>> mapChannel, string? channel = null)
            => await mapChannel(channel??typeof(T).GetCustomAttribute<MessageAttribute>(false)?.Channel??throw new MessageChannelNullException());

        internal static string GetChannel<T>(Func<string, ValueTask<string>> mapChannel, string? channel = null)
        {
            var chan = channel??typeof(T).GetCustomAttribute<MessageAttribute>(false)?.Channel??throw new MessageChannelNullException();
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
