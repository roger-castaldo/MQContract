namespace MQContract;

internal static class EnumerableExtensions
{
    public static async ValueTask WhenAll(this IEnumerable<ValueTask> tasks)
    {
        foreach (var t in tasks)
            await t;
    }

    public static async ValueTask<IEnumerable<TResult>> WhenAll<TItem, TResult>(this IEnumerable<TItem> items, Func<TItem, ValueTask<TResult>> callback)
    {
        IEnumerable<TResult> result = [];
        foreach (var t in items)
            result = result.Append(await callback(t));
        return result;
    }
}
