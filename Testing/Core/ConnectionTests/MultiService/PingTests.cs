using Moq;
using MQContract;
using MQContract.Interfaces.Service;

namespace CoreTesting.ConnectionTests.MultiService;

[TestClass]
public class PingTests
{
    private const string ServiceName = "testService";

    [TestMethod]
    public async Task TestPingAsyncWithSingleConnection()
    {
        #region Arrange
        var pingResult = new PingResult("TestHost", "1.0.0", TimeSpan.FromSeconds(5));

        var serviceConnection = new Mock<IPingableMessageServiceConnection>();
        serviceConnection.Setup(x => x.PingAsync())
            .ReturnsAsync(pingResult);

        var contractConnection = ContractConnection.MultiServiceInstance();

        contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object);
        #endregion

        #region Act
        var result = await contractConnection.PingAsync();
        #endregion

        #region Assert
        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreEqual(pingResult, result.First());
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PingAsync(), Times.Once);
        #endregion
    }

    [TestMethod]
    public async Task TestPingAsyncWithSinglePingableConnection()
    {
        #region Arrange
        var pingResult = new PingResult("TestHost", "1.0.0", TimeSpan.FromSeconds(5));

        var serviceConnection = new Mock<IPingableMessageServiceConnection>();
        serviceConnection.Setup(x => x.PingAsync())
            .ReturnsAsync(pingResult);

        var nonPingableServiceConnection = new Mock<IMessageServiceConnection>();

        var contractConnection = ContractConnection.MultiServiceInstance();

        contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object);
        contractConnection.RegisterServiceConnection("otherTestConnection", nonPingableServiceConnection.Object);
        #endregion

        #region Act
        var result = await contractConnection.PingAsync();
        #endregion

        #region Assert
        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreEqual(pingResult, result.First());
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PingAsync(), Times.Once);
        #endregion
    }

    [TestMethod]
    public async Task TestPingAsyncWithNoPingableConnection()
    {
        #region Arrange
        var serviceConnection = new Mock<IMessageServiceConnection>();

        var contractConnection = ContractConnection.MultiServiceInstance();

        contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object);
        #endregion

        #region Act
        var result = await contractConnection.PingAsync();
        #endregion

        #region Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
        #endregion

        #region Verify
        #endregion
    }

    [TestMethod]
    public async Task TestPingAsyncWithMultiplePingableConnection()
    {
        #region Arrange
        var pingResult = new PingResult("TestHost", "1.0.0", TimeSpan.FromSeconds(5));

        var serviceConnection = new Mock<IPingableMessageServiceConnection>();
        serviceConnection.Setup(x => x.PingAsync())
            .ReturnsAsync(pingResult);

        var otherServiceConnection = new Mock<IPingableMessageServiceConnection>();
        otherServiceConnection.Setup(x => x.PingAsync())
            .ReturnsAsync(pingResult);

        var contractConnection = ContractConnection.MultiServiceInstance();

        contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object);
        contractConnection.RegisterServiceConnection("otherTestConnection", otherServiceConnection.Object);
        #endregion

        #region Act
        var result = await contractConnection.PingAsync();
        #endregion

        #region Assert
        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsTrue(Array.TrueForAll(result.ToArray(), r => Equals(pingResult, r)));
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PingAsync(), Times.Once);
        otherServiceConnection.Verify(x => x.PingAsync(), Times.Once);
        #endregion
    }
}
