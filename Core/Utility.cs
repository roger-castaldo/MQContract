using System.Reflection;

namespace MQContract
{
    internal static class Utility
    {
        internal static string TypeName<T>()
            => TypeName(typeof(T));

        internal static string TypeName(Type type)
        {
            var result = type.Name;
            if (result.Contains('`'))
                result=result[..result.IndexOf('`')];
            return result;
        }

        internal static async ValueTask<object?> InvokeMethodAsync(MethodInfo method,object container, object?[]? parameters)
        {
            var valueTask = method.Invoke(container, parameters)!;
            await (Task)valueTask.GetType().GetMethod(nameof(ValueTask.AsTask))!.Invoke(valueTask, null)!;
            return valueTask.GetType().GetProperty(nameof(ValueTask<object>.Result))!.GetValue(valueTask);
        }
    }
}
