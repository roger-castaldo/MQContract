using AutomatedTesting.ServiceInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AutomatedTesting
{
    internal static class Helper
    {
        public static IServiceProvider ProduceServiceProvider(string serviceName)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IInjectableService>(new InjectedService(serviceName));
            return services.BuildServiceProvider();
        }
    }
}
