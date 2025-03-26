namespace MQContract
{
    internal static class EnumerableExtensions
    {
        public static async ValueTask WhenAll(this IEnumerable<ValueTask> tasks)
        {
            foreach (var t in tasks)
                await t;
        }

        public static async ValueTask<IEnumerable<R>> WhenAll<T, R>(this IEnumerable<T> items, Func<T, ValueTask<R>> callback)
        {
            IEnumerable<R> result = [];
            foreach (var t in items)
                result = result.Append(await callback(t));
            return result;
        }
    }
}
