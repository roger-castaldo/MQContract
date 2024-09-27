using AutomatedTesting.Messages;
using MQContract.Interfaces.Middleware;

namespace AutomatedTesting.ContractConnectionTests.Middlewares
{
    internal class ChannelChangeMiddlewareForBasicMessage : IBeforeEncodeSpecificTypeMiddleware<BasicMessage>
    {
        public static string ChangeChannel(string? channel)
            => $"{channel}-ModifiedSpecifically";
        public ValueTask<(BasicMessage message, string? channel, MessageHeader messageHeader)> BeforeMessageEncodeAsync(IContext context, BasicMessage message, string? channel, MessageHeader messageHeader)
            => ValueTask.FromResult<(BasicMessage message, string? channel, MessageHeader messageHeader)>((message, ChangeChannel(channel), messageHeader));
    }
}
