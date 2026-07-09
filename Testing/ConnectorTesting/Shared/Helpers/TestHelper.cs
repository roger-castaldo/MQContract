using ConnectorTesting.Messages;
using MQContract;
using MQContract.Interfaces;
using MQContract.Interfaces.Service;
using System.Security.Cryptography;

namespace ConnectorTesting.Helpers;

internal static class TestHelper
{
    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(100);

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

    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string RandomString(int length)
    {
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)]).ToArray());
    }

    public static async Task<IContractedConnection> CreateContractConnectionAsync(IMessageServiceConnection messageServiceConnection)
    {
        var contractConnection = await ContractConnection.Instance(messageServiceConnection)
            .RegisterMessageContextAsync(new TestMessageContext());
        Assert.IsNotNull(contractConnection);
        return contractConnection;
    }

    public static async Task Cleanup(IContractedConnection contractConnection, ISubscription? subscription = null)
    {
        if (subscription!=null)
        {
            await subscription.EndAsync();
            await subscription.DisposeAsync();
        }
        await contractConnection.CloseAsync();
        await contractConnection.DisposeAsync();
    }
}
