using MQContract.ServiceAbstractions.Messages;

namespace MQContract.Messages
{
    internal record MessageHeader : IMessageHeader
    {
        private readonly IEnumerable<KeyValuePair<string, string?>> data;
        public string? this[string tagKey]
            => data.FirstOrDefault(pair => string.Equals(pair.Key, tagKey)).Value;
        public IEnumerable<string> Keys
            => data.Select(pair => pair.Key);

        public MessageHeader(IMessageHeader? baseHeader = null, Dictionary<string, string?>? newData = null)
        {
            data = (baseHeader==null ? [] : baseHeader.Keys.Select(k => new KeyValuePair<string, string?>(k, baseHeader[k])))
                .Concat(newData==null ? [] : newData.AsEnumerable());
        }
    }
}
