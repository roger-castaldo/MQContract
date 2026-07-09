using MQContract.Interfaces.Service;

namespace ConnectorTesting.Helpers;

internal static class PingTestHelper
{
    public static async Task ExecutePingTestsAsync(IMessageServiceConnection messageServiceConnection)
    {
        var contractConnection = await TestHelper.CreateContractConnectionAsync(messageServiceConnection);

        await Task.Delay(TimeSpan.FromSeconds(30));

        var result1 = await contractConnection.PingAsync();

        Assert.IsNotNull(result1);

        await Task.Delay(TimeSpan.FromMinutes(1));

        var result2 = await contractConnection.PingAsync();

        Assert.IsNotNull(result2);

        Assert.AreNotEqual(result1, result2);

        await TestHelper.Cleanup(contractConnection);
    }

}
