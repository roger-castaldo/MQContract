using MQContract.Interfaces.Middleware;

namespace MQContract.Middleware;

internal class ChannelMappingMiddleware(ChannelMapper? channelMapper)
    : IBeforeEncodeMiddleware
{
    private async ValueTask<string?> MapChannel(Context context, string? channel)
    {
        if (channelMapper==null || channel==null)
            return channel;
        return await channelMapper.MapChannel(context.MapDirection, channel);
    }

    async ValueTask<EncodableMessage<TMessage>> IBeforeEncodeMiddleware.BeforeMessageEncodeAsync<TMessage>(IContext context, EncodableMessage<TMessage> message)
    {
        var mappedChannel = await MapChannel((Context)context, message.Channel);
        context.Activity?.AddEvent(new("MessageChannelMapped", tags: new([
            new(OpenTelemetryMiddleware.InitialChannelKey,message.Channel),
            new($"{OpenTelemetryMiddleware.KeyBase}.mappedchannel",mappedChannel)
        ])));
        return new(message.MessageHeader, message.Message, mappedChannel);
    }
}
