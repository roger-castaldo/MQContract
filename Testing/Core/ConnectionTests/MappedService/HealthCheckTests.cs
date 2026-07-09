using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using MQContract;
using MQContract.Interfaces.Service;

namespace CoreTesting.ConnectionTests.MappedService;

[TestClass]
public class HealthCheckTests
{
    private const string ServiceName = "testService";

    [TestMethod]
    public async Task TestValidHealthCheck()
    {
        #region Arrange
        var pingResult = new PingResult("TestHost", "1.0.0", TimeSpan.FromSeconds(5));

        var serviceConnection = new Mock<IPingableMessageServiceConnection>();
        serviceConnection.Setup(x => x.PingAsync())
            .ReturnsAsync(pingResult);

        var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
        #endregion

        #region Act
        var healthCheck = contractConnection.HealthCheck;
        Assert.IsNotNull(healthCheck);
        var checkResult = await healthCheck.CheckHealthAsync(new(), TestContext.CancellationToken);
        #endregion

        #region Assert
        Assert.AreEqual(HealthStatus.Healthy, checkResult.Status);
        Assert.AreEqual(Constants.HealthyDescription, checkResult.Description);
        Assert.IsTrue(checkResult.Data.TryGetValue(ServiceName, out var value));
        Assert.IsInstanceOfType<Dictionary<string, object>>(value);
        var dict = (Dictionary<string, object>)value;
        Assert.AreEqual(pingResult.Host, dict["Host"]);
        Assert.AreEqual(pingResult.Version, dict["Version"]);
        Assert.AreEqual(pingResult.ResponseTime, dict["ResponseTime"]);
        Assert.AreEqual(healthCheck, contractConnection.HealthCheck);
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PingAsync(), Times.Once);
        #endregion
    }

    [TestMethod]
    public async Task TestPingExceptionUnhealthyCheck()
    {
        #region Arrange
        var error = new PingFailedException("Ping failed");

        var serviceConnection = new Mock<IPingableMessageServiceConnection>();
        serviceConnection.Setup(x => x.PingAsync())
            .ThrowsAsync(error);

        var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
        #endregion

        #region Act
        var healthCheck = contractConnection.HealthCheck;
        Assert.IsNotNull(healthCheck);
        var checkResult = await healthCheck.CheckHealthAsync(new(), TestContext.CancellationToken);
        #endregion

        #region Assert
        Assert.AreEqual(HealthStatus.Unhealthy, checkResult.Status);
        Assert.AreEqual(Constants.UnHealthyDescription, checkResult.Description);
        Assert.IsTrue(checkResult.Data.TryGetValue(ServiceName, out var value));
        Assert.IsInstanceOfType<Dictionary<string, object>>(value);
        var dict = (Dictionary<string, object>)value;
        Assert.AreEqual(error.Message, dict["Error"]);
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PingAsync(), Times.Once);
        #endregion
    }

    [TestMethod]
    public void TestInabilityToPerformHealthCheckWithNonPingableService()
    {
        #region Arrange
        var serviceConnection = new Mock<IMessageServiceConnection>();

        var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
        #endregion

        #region Act
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => _ = contractConnection.HealthCheck);
        #endregion

        #region Assert
        Assert.IsNotNull(error);
        Assert.AreEqual("serviceConnectionList", error.ParamName);
        Assert.StartsWith("No Pingable service connections provided, cannot provide health checks", error.Message);
        #endregion

        #region Verify
        #endregion
    }

    [TestMethod]
    public async Task TestValidHealthCheckMultiplePingableServices()
    {
        #region Arrange
        var pingResult = new PingResult("TestHost", "1.0.0", TimeSpan.FromSeconds(5));

        var serviceConnection = new Mock<IPingableMessageServiceConnection>();
        serviceConnection.Setup(x => x.PingAsync())
            .ReturnsAsync(pingResult);

        var contractConnection = ContractConnection.MappedServiceInstance()
            .RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object)
            .RegisterServiceConnection((props) => true, $"{ServiceName}2", serviceConnection.Object);
        #endregion

        #region Act
        var healthCheck = contractConnection.HealthCheck;
        Assert.IsNotNull(healthCheck);
        var checkResult = await healthCheck.CheckHealthAsync(new(), TestContext.CancellationToken);
        #endregion

        #region Assert
        Assert.AreEqual(HealthStatus.Healthy, checkResult.Status);
        Assert.AreEqual(Constants.HealthyDescription, checkResult.Description);
        Assert.IsTrue(checkResult.Data.TryGetValue(ServiceName, out var value));
        Assert.IsInstanceOfType<Dictionary<string, object>>(value);
        var dict = (Dictionary<string, object>)value;
        Assert.AreEqual(pingResult.Host, dict["Host"]);
        Assert.AreEqual(pingResult.Version, dict["Version"]);
        Assert.AreEqual(pingResult.ResponseTime, dict["ResponseTime"]);
        Assert.IsTrue(checkResult.Data.TryGetValue($"{ServiceName}2", out var value2));
        Assert.IsInstanceOfType<Dictionary<string, object>>(value2);
        var dict2 = (Dictionary<string, object>)value2;
        Assert.AreEqual(pingResult.Host, dict2["Host"]);
        Assert.AreEqual(pingResult.Version, dict2["Version"]);
        Assert.AreEqual(pingResult.ResponseTime, dict2["ResponseTime"]);
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PingAsync(), Times.Exactly(2));
        #endregion
    }

    [TestMethod]
    public async Task TestDegradedHealthCheckMultiplePingableServices()
    {
        #region Arrange
        var pingResult = new PingResult("TestHost", "1.0.0", TimeSpan.FromSeconds(5));
        var error = new Exception("Ping failed");

        var serviceConnection = new Mock<IPingableMessageServiceConnection>();
        serviceConnection.Setup(x => x.PingAsync())
            .ReturnsAsync(pingResult);

        var serviceConnection2 = new Mock<IPingableMessageServiceConnection>();
        serviceConnection2.Setup(x => x.PingAsync())
            .ThrowsAsync(error);

        var contractConnection = ContractConnection.MappedServiceInstance()
            .RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object)
            .RegisterServiceConnection((props) => true, $"{ServiceName}2", serviceConnection2.Object);
        #endregion

        #region Act
        var healthCheck = contractConnection.HealthCheck;
        Assert.IsNotNull(healthCheck);
        var checkResult = await healthCheck.CheckHealthAsync(new(), TestContext.CancellationToken);
        #endregion

        #region Assert
        Assert.AreEqual(HealthStatus.Degraded, checkResult.Status);
        Assert.AreEqual(Constants.DegradedDescription, checkResult.Description);
        Assert.IsTrue(checkResult.Data.TryGetValue(ServiceName, out var value));
        Assert.IsInstanceOfType<Dictionary<string, object>>(value);
        var dict = (Dictionary<string, object>)value;
        Assert.AreEqual(pingResult.Host, dict["Host"]);
        Assert.AreEqual(pingResult.Version, dict["Version"]);
        Assert.AreEqual(pingResult.ResponseTime, dict["ResponseTime"]);
        Assert.IsTrue(checkResult.Data.TryGetValue($"{ServiceName}2", out var value2));
        Assert.IsInstanceOfType<Dictionary<string, object>>(value2);
        var dict2 = (Dictionary<string, object>)value2;
        Assert.AreEqual(error.Message, dict2["Error"]);
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PingAsync(), Times.Once);
        serviceConnection2.Verify(x => x.PingAsync(), Times.Once);
        #endregion
    }

    public TestContext TestContext { get; set; }
}
