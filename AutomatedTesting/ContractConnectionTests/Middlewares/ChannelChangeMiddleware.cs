using MQContract.Interfaces.Middleware;

namespace AutomatedTesting.ContractConnectionTests.Middlewares
{
    internal class ChannelChangeMiddleware : IBeforeEncodeMiddleware
    {
        public static string ChangeChannel(string? channel)
            => $"{channel}-Modified";
        public ValueTask<(T message, string? channel, MessageHeader messageHeader)> BeforeMessageEncodeAsync<T>(IContext context, T message, string? channel, MessageHeader messageHeader)
            => ValueTask.FromResult((message, ChangeChannel(channel), messageHeader));
    }
}
