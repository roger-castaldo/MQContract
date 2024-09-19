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

        public static ReceivedServiceMessage ProduceReceivedServiceMessage(ServiceMessage message, string? messageTypeID = null)
            => new(message.ID, messageTypeID??message.MessageTypeID, message.Channel, message.Header, message.Data);

        public static ServiceQueryResult ProduceQueryResult(ServiceMessage message)
            => new(message.ID, message.Header, message.MessageTypeID, message.Data);

        private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(5);

        public static async Task<bool> WaitForCount<T>(IEnumerable<T> values,int count,TimeSpan maxTime)
            where T : class
        {
            var task = new Task(() =>
            {
                while (values.Count()!=count)
                    Task.Delay(Delay).Wait();
            });
            task.Start();
            return await Task.WhenAny(task, Task.Delay(maxTime)) == task;
        }
    }
}
