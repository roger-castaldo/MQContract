using MQContract.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Connections
{
    internal record DecodeServiceMessageResult<T>
    {
        public T? Message { get; private init; } = default(T?);
        public MessageHeader? Header { get; private init; } = null;
        public MessageFilterResult FilterResult { get; private init; } = MessageFilterResult.Allow;

        public static DecodeServiceMessageResult<T> ProduceResult(T message, MessageHeader messageHeader)
            => new()
            {
                Message = message,
                Header = messageHeader
            };

        public static DecodeServiceMessageResult<T> ProduceResult(MessageFilterResult messageFilterResult)
            => new() { FilterResult = messageFilterResult };
    }
}
