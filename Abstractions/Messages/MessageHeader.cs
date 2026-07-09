using System.Buffers;

namespace MQContract.Messages;

/// <summary>
/// Houses additional headers to be passed through or that were passed along the service message
/// </summary>
public sealed class MessageHeader : IDisposable
{
    private static readonly StringComparer comparer = StringComparer.InvariantCultureIgnoreCase;

    private const int DefaultCapacity = 4;

    private KeyValuePair<string, string>[] buffer;
    private int count;

    /// <summary>
    /// The current count of values in the collection
    /// </summary>
    public int Count => count;

    /// <summary>
    /// Constructor to create a MessageHeader instance without any inital headers
    /// </summary>
    public MessageHeader()
        : this([])
    { }

    /// <summary>
    /// Constructor to create a MessageHeader instance merging and existing header with new header values. The existing header values will be overwritten by the new header values if there are any key conflicts, otherwise the new header values will be added to the existing header values.
    /// </summary>
    /// <param name="originalHeader">The original header to merge with the new header values</param>
    /// <param name="headers">The desired data for the header</param>
    public MessageHeader(MessageHeader originalHeader, IEnumerable<KeyValuePair<string, string?>> headers)
        : this(
              headers
              .Concat(
                  originalHeader.buffer.Take(originalHeader.count)
                  .Where(kvp => !headers.Any(h => comparer.Equals(kvp.Key, h.Key)))
                  .Select(kvp => new KeyValuePair<string, string?>(kvp.Key, kvp.Value))
              )
        )
    { }


    /// <summary>
    /// Constructor to create a MessageHeader instance using initial data values
    /// </summary>
    /// <param name="headers">The desired data for the header</param>
    public MessageHeader(IEnumerable<KeyValuePair<string, string?>> headers)
    {
        var cleanHeaders = headers.Where(kvp => kvp.Value is not null)
              .Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value!))
              .ToArray();
        buffer = ArrayPool<KeyValuePair<string, string>>.Shared.Rent(Math.Max(cleanHeaders.Length, DefaultCapacity));
        cleanHeaders.CopyTo(this.buffer, 0);
        count = cleanHeaders.Length;
    }

    /// <summary>
    /// Called to obtain a header value for the given key if it exists, or set a header value for the given key. Setting a value for an existing key will overwrite the existing value, while setting a value for a non-existing key will add a new header to the collection.
    /// </summary>
    /// <param name="key">The unique header key to get the value for</param>
    /// <returns>The value for the given key or null if not found</returns>
    public string? this[string key]
    {
        get {
            for (int i = 0; i < count; i++)
            {
                if (comparer.Equals(buffer[i].Key, key))
                    return buffer[i].Value;
            }
            return null;
        }
        set
        {
            for (int i = 0; i < count; i++)
            {
                if (comparer.Equals(buffer[i].Key, key))
                {
                    if (value is null)
                    {
                        count--;
                        buffer[i] = buffer[count];
                        buffer[count] = default!;
                    } else
                        buffer[i] = new(key, value);
                    return;
                }
            }
            if (value is null)
                return;
            EnsureCapacity();
            buffer[count++] = new(key, value);
        }
    }

    /// <summary>
    /// A list of the available keys in the header
    /// </summary>
    public IEnumerable<string> Keys
    {
        get
        {
            var span = AsSpan();
            var result = new string[span.Length];
            for (int i = 0; i < span.Length; i++)
                result[i] = span[i].Key;
            return result;
        }
    }

    /// <summary>
    /// The list of values returned as a span for the underlying services to read them all at once. This is more efficient than using the indexer for each key when the underlying service supports it.
    /// </summary>
    public ReadOnlySpan<KeyValuePair<string, string>> AsSpan()
        => buffer.AsSpan(0, count);

    /// <summary>
    /// Execute an action for each header entry without exposing the underlying span.
    /// This avoids allocations and also avoids capturing a ref struct in async methods.
    /// </summary>
    /// <param name="action">Action to execute for each header key/value pair.</param>
    public void ForEach(Action<KeyValuePair<string, string>> action)
    {
        var span = AsSpan();
        for (var i = 0; i < span.Length; i++)
            action(span[i]);
    }

    /// <summary>
    /// Projects each key-value pair in the collection into a new form by applying the specified selector function.
    /// </summary>
    /// <typeparam name="T">The type of the elements returned by the selector function.</typeparam>
    /// <param name="selector">A function to apply to each key-value pair in the collection to produce the result element.</param>
    /// <returns>An enumerable collection of elements of type T resulting from applying the selector function to each
    /// key-value pair.</returns>
    public IEnumerable<T> Select<T>(Func<KeyValuePair<string, string>, T> selector)
    {
        var span = AsSpan();
        var result = new T[span.Length];
        for (var i = 0; i < span.Length; i++)
            result[i] = selector(span[i]);
        return result;
    }

    /// <summary>
    /// Returns the header key/value pairs as an enumerable list. This is less efficient than using the AsSpan method, but is more convenient for use in LINQ queries and other scenarios where an enumerable is required.
    /// </summary>
    /// <returns>The header key/value pairs as an enumerable value</returns>
    public IEnumerable<KeyValuePair<string, string>> AsEnumerable()
    {
        var span = AsSpan();
        var result = new KeyValuePair<string, string>[span.Length];
        span.CopyTo(result);
        return result;
    }

    private void EnsureCapacity()
    {
        if (count < buffer.Length)
            return;

        var newBuffer = ArrayPool<KeyValuePair<string, string>>.Shared.Rent(buffer.Length + DefaultCapacity);

        buffer.AsSpan(0, count).CopyTo(newBuffer);

        ArrayPool<KeyValuePair<string, string>>.Shared.Return(buffer, clearArray: true);
        buffer = newBuffer;
    }

    void IDisposable.Dispose()
    {
        ArrayPool<KeyValuePair<string, string>>.Shared.Return(buffer, clearArray: true);
        buffer = [];
        count = 0;
    }
}
