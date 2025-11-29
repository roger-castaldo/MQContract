using MQContract.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Interfaces.Middleware
{
    public readonly record struct DecodableMessage(MessageHeader MessageHeader,ReadOnlyMemory<byte> Data);

    public readonly record struct DecodedMessage<TMessage>(MessageHeader MessageHeader, TMessage Message);

    public readonly record struct EncodableMessage<TMessage>(MessageHeader MessageHeader, TMessage Message, string? Channel);
}
