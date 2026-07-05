using MQContract.Interfaces.Middleware;

namespace CoreTesting.ConnectionTests.Middlewares
{
    internal class ChannelChangeMiddleware : IBeforeEncodeMiddleware
    {
        public static string ChangeChannel(string? channel)
            => $"{channel}-Modified";
        ValueTask<EncodableMessage<TMessage>> IBeforeEncodeMiddleware.BeforeMessageEncodeAsync<TMessage>(IContext context, EncodableMessage<TMessage> message)
            => ValueTask.FromResult<EncodableMessage<TMessage>>(new(message.MessageHeader, message.Message, ChangeChannel(message.Channel)));
    }
}
