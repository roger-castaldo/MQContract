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

        internal static async ValueTask<object?> InvokeMethodAsync(MethodInfo method, object container, object?[]? parameters)
        {
            var valueTask = method.Invoke(container, parameters)!;
            await (Task)valueTask.GetType().GetMethod(nameof(ValueTask.AsTask))!.Invoke(valueTask, null)!;
            return valueTask.GetType().GetProperty(nameof(ValueTask<object>.Result))!.GetValue(valueTask);
        }

        internal async static ValueTask<string> GetChannelAsync<TMessage>(Func<string, ValueTask<string>> mapChannel, MessageContext context, string? channel = null)
            => await mapChannel(channel??context.MessageChannel<TMessage>()??throw new MessageChannelNullException());

        internal static string GetChannel<TMessage>(Func<string, ValueTask<string>> mapChannel, MessageContext context, string? channel = null)
        {
            var chan = channel??context.MessageChannel<TMessage>()??throw new MessageChannelNullException();
            var tsk = mapChannel(chan).AsTask();
            tsk.Wait();
            return tsk.Result;
        }
    }
}
