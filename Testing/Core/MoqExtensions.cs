using Moq.Language.Flow;

namespace CoreTesting;

public static class MoqExtensions
{
    public static IReturnsResult<T> ReturnsInOrder<T, TResult>(
        this ISetup<T, TResult> setup, params TResult[] results) where T : class
    {
        var queue = new Queue<TResult>(results);
#pragma warning disable CS8603 // Possible null reference return.
        return setup.Returns(() => queue.Count > 0 ? queue.Dequeue() : default);
#pragma warning restore CS8603 // Possible null reference return.
    }
}
