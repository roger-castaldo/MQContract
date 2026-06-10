using ConnectorTesting.Messages;
using MQContract;
using MQContract.Interfaces;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace ConnectorTesting.Helpers;

internal static class PingTestHelper
{
    public static async Task ExecutePingTestsAsync(IMessageServiceConnection messageServiceConnection)
    {
        await using var contractConnection = await ContractConnection.Instance(messageServiceConnection)
            .RegisterMessageContextAsync(new TestMessageContext());

        Assert.IsNotNull(contractConnection);

        await Task.Delay(TimeSpan.FromSeconds(30));

        var result1 = await contractConnection.PingAsync();

        Assert.IsNotNull(result1);

        await Task.Delay(TimeSpan.FromSeconds(30));

        var result2 = await contractConnection.PingAsync();

        Assert.IsNotNull(result2);

        Assert.AreNotEqual(result1, result2);

        await contractConnection.CloseAsync();
    }

}
