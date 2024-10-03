namespace MQContract.Messages
{
    /// <summary>
    /// Houses additional headers to be passed through or that were passed along the service message
    /// </summary>
    /// <param name="data">A list of KeyValuePairs that make up the header</param>
    public sealed class MessageHeader(IEnumerable<KeyValuePair<string, string>> data)
    {
        /// <summary>
        /// Constructor to create the MessageHeader class using a Dictionary
        /// </summary>
        /// <param name="headers">The desired data for the header</param>
        public MessageHeader(Dictionary<string, string?>? headers)
            : this(headers?.AsEnumerable().Select(pair=>new KeyValuePair<string,string>(pair.Key,pair.Value??string.Empty))?? []) { }

        /// <summary>
        /// Constructor to create a merged message header with taking the original and appending the new values
        /// </summary>
        /// <param name="originalHeader">The base header to use</param>
        /// <param name="appendedHeader">The additional properties to add</param>
        public MessageHeader(MessageHeader? originalHeader, Dictionary<string, string?>? appendedHeader)
            : this(
                  (appendedHeader?.AsEnumerable()
                  .Where(pair=>pair.Value!=null)
                  .Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value!))?? [])
                  .Concat(originalHeader?.Keys
                      .Where(k => !(appendedHeader?? []).Any(pair=>Equals(k,pair.Key)))
                      .Select(k => new KeyValuePair<string, string>(k, originalHeader?[k]!))?? [])
                  .DistinctBy(k => k.Key)
            )
        { }
        
        /// <summary>
        /// Called to obtain a header value for the given key if it exists
        /// </summary>
        /// <param name="tagKey">The unique header key to get the value for</param>
        /// <returns>The value for the given key or null if not found</returns>
        public string? this[string tagKey] 
            => data.Where(p=>Equals(p.Key,tagKey))
            .Select(p=>p.Value)
            .FirstOrDefault();

        /// <summary>
        /// A list of the available keys in the header
        /// </summary>
        public IEnumerable<string> Keys 
            => data.Select(p=>p.Key);
    }
}
