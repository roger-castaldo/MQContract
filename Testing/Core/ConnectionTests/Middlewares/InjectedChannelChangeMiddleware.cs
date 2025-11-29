using AutomatedTesting.ServiceInjection;
using MQContract.Interfaces.Middleware;

namespace AutomatedTesting.ContractConnectionTests.Middlewares
{
    internal class InjectedChannelChangeMiddleware(IInjectableService service)
        : IBeforeEncodeMiddleware
    {
        ValueTask<EncodableMessage<TMessage>> IBeforeEncodeMiddleware.BeforeMessageEncodeAsync<TMessage>(IContext context, EncodableMessage<TMessage> message)
            => ValueTask.FromResult<EncodableMessage<TMessage>>(new(message.MessageHeader, message.Message, service.Name));
    }
}
