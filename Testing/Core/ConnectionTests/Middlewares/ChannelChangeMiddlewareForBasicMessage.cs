using CoreTesting.Messages;
using MQContract.Interfaces.Middleware;

namespace CoreTesting.ConnectionTests.Middlewares
{
    internal class ChannelChangeMiddlewareForBasicMessage : IBeforeEncodeSpecificTypeMiddleware<BasicMessage>
    {
        public static string ChangeChannel(string? channel)
            => $"{channel}-ModifiedSpecifically";
        ValueTask<EncodableMessage<BasicMessage>> IBeforeEncodeSpecificTypeMiddleware<BasicMessage>.BeforeMessageEncodeAsync(IContext context, EncodableMessage<BasicMessage> message)
            => ValueTask.FromResult<EncodableMessage<BasicMessage>>(new(message.MessageHeader, message.Message, ChangeChannel(message.Channel)));
    }
}
