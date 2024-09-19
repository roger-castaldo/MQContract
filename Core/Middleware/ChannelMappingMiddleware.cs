using MQContract.Interfaces.Middleware;
using MQContract.Messages;

namespace MQContract.Middleware
{
    internal class ChannelMappingMiddleware(ChannelMapper? channelMapper) 
        : IBeforeEncodeMiddleware
    {
        private async ValueTask<string?> MapChannel(Context context,string? channel)
        {
            if (channelMapper==null || channel==null)
                return channel;
            return await channelMapper.MapChannel(context.MapDirection, channel);
        }

        public async ValueTask<(T message, string? channel, MessageHeader messageHeader)> BeforeMessageEncodeAsync<T>(IContext context, T message, string? channel, MessageHeader messageHeader)
            => (message, await MapChannel((Context)context,channel), messageHeader);
    }
}
