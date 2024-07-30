namespace MQContract.Messages
{
    public sealed class MessageHeader(IEnumerable<KeyValuePair<string,string>> data)
        : IMessageHeader
    {
        public MessageHeader(Dictionary<string, string?>? headers)
            : this(headers?.AsEnumerable().Select(pair=>new KeyValuePair<string,string>(pair.Key,pair.Value??string.Empty))?? []) { }

        public MessageHeader(IMessageHeader? originalHeader, Dictionary<string, string?>? appendedHeader)
            : this(
                  (appendedHeader?.AsEnumerable().Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value??string.Empty))?? [])
                  .Concat(originalHeader?.Keys.Select(k => new KeyValuePair<string, string>(k, originalHeader?[k]!))?? [])
                  .DistinctBy(k => k.Key)
            )
        { }
        
        public string? this[string tagKey] 
            => data.Where(p=>Equals(p.Key,tagKey))
            .Select(p=>p.Value)
            .FirstOrDefault();

        public IEnumerable<string> Keys 
            => data.Select(p=>p.Key);
    }
}
