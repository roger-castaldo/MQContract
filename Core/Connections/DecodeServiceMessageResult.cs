using MQContract.Messages;

namespace MQContract.Connections;

internal record DecodeServiceMessageResult<TMessage>
{
    public TMessage? Message { get; private init; } = default(TMessage?);
    public MessageHeader? Header { get; private init; } = null;
    public MessageFilterResult FilterResult { get; private init; } = MessageFilterResult.Allow;

    public static DecodeServiceMessageResult<TMessage> ProduceResult(TMessage message, MessageHeader messageHeader)
        => new()
        {
            Message = message,
            Header = messageHeader
        };

    public static DecodeServiceMessageResult<TMessage> ProduceResult(MessageFilterResult messageFilterResult)
        => new() { FilterResult = messageFilterResult };
}
