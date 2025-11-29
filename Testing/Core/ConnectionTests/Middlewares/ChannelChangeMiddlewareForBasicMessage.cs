using AutomatedTesting.Messages;
using MQContract.Interfaces.Middleware;

namespace AutomatedTesting.ContractConnectionTests.Middlewares
{
    internal class ChannelChangeMiddlewareForBasicMessage : IBeforeEncodeSpecificTypeMiddleware<BasicMessage>
    {
        public static string ChangeChannel(string? channel)
            => $"{channel}-ModifiedSpecifically";
        ValueTask<EncodableMessage<BasicMessage>> IBeforeEncodeSpecificTypeMiddleware<BasicMessage>.BeforeMessageEncodeAsync(IContext context, EncodableMessage<BasicMessage> message)
            => ValueTask.FromResult<EncodableMessage<BasicMessage>>(new(message.MessageHeader, message.Message, ChangeChannel(message.Channel)));
    }
}
