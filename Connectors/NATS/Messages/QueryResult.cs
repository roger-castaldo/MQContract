using MQContract.Messages;
using NATS.Client.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.NATS.Messages
{
    internal class QueryResult(NatsMsg<NatsQueryResponseMessage> result) : IServiceQueryResult
    {
        public string ID => result.Data?.ID??string.Empty;

        public string MessageTypeID => result.Data?.MessageTypeID??string.Empty;

        public string Channel => result.Subject;

        public IMessageHeader Header { get; private init; } = new MessageHeader(result.Headers);

        public ReadOnlyMemory<byte> Data => result.Data?.Data??new ReadOnlyMemory<byte>();

        public bool IsError => !string.IsNullOrWhiteSpace(Error);

        public string? Error => result.Error?.Message??result.Data?.Error;
    }
}
