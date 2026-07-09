using CoreTesting.ServiceInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;

namespace CoreTesting;

internal static class Helper
{
    private const string ValidCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

    public static string GenerateRandomString(int length)
        => RandomNumberGenerator.GetString(ValidCharacters, length);

    public static IServiceProvider ProduceServiceProvider(string serviceName)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IInjectableService>(new InjectedService(serviceName));
        return services.BuildServiceProvider();
    }

    public static ReceivedServiceMessage ProduceReceivedServiceMessage(ServiceMessage message, string? messageTypeID = null, Func<ValueTask>? acknowledge = null)
        => new(message.ID, messageTypeID??message.MessageTypeID, message.Channel, message.Header, message.Data, acknowledge);

    public static ServiceQueryResult ProduceQueryResult(ServiceMessage? message)
        => new(message?.ID??string.Empty, message?.Header??new([]), message?.MessageTypeID??string.Empty, message?.Data?? Array.Empty<byte>());

    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(5);

    public static async Task<bool> WaitForCount<T>(IEnumerable<T> values, int count, TimeSpan maxTime)
        where T : class
    {
        var task = new Task(() =>
        {
            while (values.Count()<count)
                Task.Delay(Delay).Wait();
        });
        task.Start();
        return (await Task.WhenAny(task, Task.Delay(maxTime))) == task || values.Count()>=count;
    }
}
