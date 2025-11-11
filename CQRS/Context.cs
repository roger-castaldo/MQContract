using MQContract.Messages;

namespace MQContract.CQRS
{
    public sealed record Context
    {
        private const string MessageIdHeaderKey = "_messageId";
        private const string CorrelationIdHeaderKey = "_correlationId";
        private const string CausationIdHeaderKey = "_causationId";
        private readonly MessageHeader messageHeader;
        private readonly Dictionary<string, string?> properties = [];

        public Context()
        {
            messageHeader = new([
                new KeyValuePair<string,string>(MessageIdHeaderKey, Guid.NewGuid().ToString()),
                new KeyValuePair<string,string>(CorrelationIdHeaderKey, Guid.NewGuid().ToString())
           ]);
        }

        internal Context(MessageHeader messageHeader)
        {
            this.messageHeader = messageHeader;
            if(string.IsNullOrWhiteSpace(this.messageHeader[MessageIdHeaderKey]))
                properties.Add(MessageIdHeaderKey, Guid.NewGuid().ToString());
        }

        public string? this[string key]
        {
            get
            {
                if (properties.TryGetValue(key, out var result))
                    return result;
                return messageHeader[key];
            }
            set
            {
                properties.Remove(key);
                properties.Add(key, value);
            }
        }

        public Guid MessageId => Guid.Parse(this[MessageIdHeaderKey]!);
        public Guid CorrelationId => Guid.Parse(this[CorrelationIdHeaderKey]!);
        public Guid? CausationId => (string.IsNullOrWhiteSpace(this[CausationIdHeaderKey]) ? null : Guid.Parse(this[CausationIdHeaderKey]!));

        internal Context CloneToChild()
            => new(new MessageHeader(messageHeader, new Dictionary<string, string?>(
                    properties
                        .AsEnumerable()
                        .Where(pair => !Equals(pair.Key, CausationIdHeaderKey) && !Equals(pair.Key, MessageIdHeaderKey))
                        .Concat([
                            new(MessageIdHeaderKey,Guid.NewGuid().ToString()),
                            new(CausationIdHeaderKey,MessageId.ToString())
                        ])
            )));

        internal MessageHeader AsMessageHeader()
            => new(messageHeader, properties);

        internal Dictionary<string,string?> AsDictionary()
        {
            var header = AsMessageHeader();
            return new(
                header.Keys
                .Where(k => !string.IsNullOrWhiteSpace(header[k]))
                .Select(k=>new KeyValuePair<string,string?>(k,header[k]!))
            );
        }
                
    }
}
