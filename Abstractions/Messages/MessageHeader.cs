using System.Buffers;

namespace MQContract.Messages
{
    /// <summary>
    /// Houses additional headers to be passed through or that were passed along the service message
    /// </summary>
    public sealed class MessageHeader : IDisposable
    {
        private readonly StringComparer comparer = StringComparer.InvariantCultureIgnoreCase;

        private const int DefaultCapacity = 4;

        private KeyValuePair<string, string>[] buffer;
        private int count;

        /// <summary>
        /// Constructor to create a MessageHeader instance without any inital headers
        /// </summary>
        public MessageHeader()
            : this([])
        {}

        /// <summary>
        /// Constructor to create a MessageHeader instance using initial data values
        /// </summary>
        /// <param name="headers">The desired data for the header</param>
        public MessageHeader(IEnumerable<KeyValuePair<string, string>> headers)
        {
            buffer = ArrayPool<KeyValuePair<string, string>>.Shared.Rent(Math.Max(headers.Count(),DefaultCapacity));
            headers.ToArray().CopyTo(this.buffer, 0);
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
                        }else
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
                for(int i = 0; i < count; i++)
                {
                    yield return buffer[i].Key;
                }
            }
        }

        /// <summary>
        /// The list of values returned as a span for the underlying services to read them all at once. This is more efficient than using the indexer for each key when the underlying service supports it.
        /// </summary>
        public ReadOnlySpan<KeyValuePair<string, string>> AsSpan()
            => buffer.AsSpan(0, count);

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
}
