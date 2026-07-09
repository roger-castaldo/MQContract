using Microsoft.Extensions.Logging;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Factories;
using MQContract.Interfaces.Messages;
using MQContract.Messages;

namespace MQContract.Factories;

internal class MessageTypeFactory<TMessage>
    : IMessageFactory<TMessage>
{
    private readonly Func<TMessage, ValueTask<byte[]>> encodeMessage;
    private readonly Func<Stream, ValueTask<TMessage?>> decodeMessage;
    public bool IgnoreMessageHeader { get; private init; }

    private readonly string messageTypeID;
    private readonly MessageContext context;
    private readonly IMessageEncoder? globalMessageEncoder;
    private readonly IServiceProvider? serviceProvider;
    public string? MessageChannel { get; private init; }

    public MessageTypeFactory(IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider, bool ignoreMessageHeader, MessageContext context)
    {
        this.context=context;
        messageTypeID = context.MessageID<TMessage>();
        this.globalMessageEncoder = globalMessageEncoder;
        this.serviceProvider  = serviceProvider;
        MessageChannel = context.MessageChannel<TMessage>();
        IgnoreMessageHeader = ignoreMessageHeader;
        (encodeMessage, decodeMessage) = context.GetEncodingCallbacks<TMessage>(globalMessageEncoder, serviceProvider);
        context.PrimeConverters<TMessage>(globalMessageEncoder, serviceProvider);
    }

    public async ValueTask<ServiceMessage> ConvertMessageAsync(TMessage message, bool ignoreChannel, string? channel, MessageHeader messageHeader, string? messageID)
    {
        if (string.IsNullOrWhiteSpace(channel)&&!ignoreChannel)
            throw new MessageChannelNullException();

        return new ServiceMessage(
            messageID??Guid.NewGuid().ToString(),
            messageTypeID,
            channel??string.Empty,
            messageHeader,
            await encodeMessage(message)
        );
    }

    public async ValueTask<TMessage?> ConvertMessageAsync(ILogger? logger, IEncodedMessage message)
    {
        if (!IgnoreMessageHeader)
#pragma warning disable S3236 // Caller information arguments should not be provided explicitly
            ArgumentNullException.ThrowIfNullOrWhiteSpace(message.MessageTypeID, nameof(message.MessageTypeID));
#pragma warning restore S3236 // Caller information arguments should not be provided explicitly
        if (Equals(ErrorServiceMessage.MessageTypeID, message.MessageTypeID))
            throw ErrorServiceMessage.DecodeError(message.Data);
        TMessage? result;
        if (IgnoreMessageHeader || string.Equals(messageTypeID, message.MessageTypeID, StringComparison.InvariantCultureIgnoreCase))
        {
            using var ms = new MemoryStream(message.Data.ToArray(), 0, message.Data.Length, false, true);
            result = await decodeMessage(ms);
        }
        else
        {
            var converter = context.GetMessageConverter<TMessage>(message.MessageTypeID, globalMessageEncoder, serviceProvider);
            if (converter==null)
                throw new InvalidCastException();
            result = (TMessage?)await converter(message);
        }
        if (Equals(result, default(TMessage?)))
            throw new MessageConversionException(typeof(TMessage));
        return result;
    }
}
