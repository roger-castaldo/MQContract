using MQContract.Attributes;
using System.Reflection;

namespace MQContract
{
    internal static class Utility
    {
        internal static string MessageTypeName<T>()
            => MessageTypeName(typeof(T));

        internal static string MessageTypeName(Type messageType)
            => messageType.GetCustomAttributes<MessageNameAttribute>().Select(mn => mn.Value).FirstOrDefault(TypeName(messageType));

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
            =>messageType.GetCustomAttributes<MessageVersionAttribute>().Select(mc => mc.Version.ToString()).FirstOrDefault("0.0.0.0");

        internal static async ValueTask<object?> InvokeMethodAsync(MethodInfo method,object container, object?[]? parameters)
        {
            var valueTask = method.Invoke(container, parameters)!;
            await (Task)valueTask.GetType().GetMethod(nameof(ValueTask.AsTask))!.Invoke(valueTask, null)!;
            return valueTask.GetType().GetProperty(nameof(ValueTask<object>.Result))!.GetValue(valueTask);
        }
    }
}
