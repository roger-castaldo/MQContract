using AutomatedTesting.ServiceInjection;
using MQContract.Interfaces.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedTesting.ContractConnectionTests.Middlewares
{
    internal class InjectedChannelChangeMiddleware(IInjectableService service) 
        : IBeforeEncodeMiddleware
    {
        public ValueTask<(T message, string? channel, MessageHeader messageHeader)> BeforeMessageEncodeAsync<T>(IContext context, T message, string? channel, MessageHeader messageHeader)
            => ValueTask.FromResult((message, service.Name, messageHeader));
    }
}
