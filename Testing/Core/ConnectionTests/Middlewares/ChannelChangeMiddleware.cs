using MQContract.Interfaces.Middleware;

namespace AutomatedTesting.ContractConnectionTests.Middlewares
{
    internal class ChannelChangeMiddleware : IBeforeEncodeMiddleware
    {
        public static string ChangeChannel(string? channel)
            => $"{channel}-Modified";
        public ValueTask<(T message, string? channel, MessageHeader messageHeader)> BeforeMessageEncodeAsync<T>(IContext context, T message, string? channel, MessageHeader messageHeader)
            => ValueTask.FromResult<(T message, string? channel, MessageHeader messageHeader)>((message, ChangeChannel(channel), messageHeader));

        ValueTask<EncodableMessage<TMessage>> IBeforeEncodeMiddleware.BeforeMessageEncodeAsync<TMessage>(IContext context, EncodableMessage<TMessage> message)
            => ValueTask.FromResult<EncodableMessage<TMessage>>(new(message.MessageHeader, message.Message, ChangeChannel(message.Channel)));
    }
}
