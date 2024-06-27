using AutomatedTesting.ServiceInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
