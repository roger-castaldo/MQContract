using Google.Protobuf.Collections;
using MQContract.Messages;

namespace MQContract.KubeMQ.Messages
{
    internal class MessageHeader(MapField<string, string>? tags=null) : IMessageHeader
    {

        public string? this[string tagKey] 
            => (tags?.TryGetValue(tagKey,out var result)??false ? result : null);

        public IEnumerable<string> Keys 
            => tags?.Keys?? [];
    }
}
