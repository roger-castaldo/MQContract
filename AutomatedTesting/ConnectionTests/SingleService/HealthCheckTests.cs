using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using MQContract;
using MQContract.Interfaces.Service;

namespace AutomatedTesting.ConnectionTests.SingleService
{
    [TestClass]
    public class HealthCheckTests
    {
        [TestMethod]
        public async Task TestValidHealthCheck()
        {
            #region Arrange
            var pingResult = new PingResult("TestHost", "1.0.0", TimeSpan.FromSeconds(5));

            var serviceConnection = new Mock<IPingableMessageServiceConnection>();
            serviceConnection.Setup(x => x.PingAsync())
                .ReturnsAsync(pingResult);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var healthCheck = contractConnection.HealthCheck;
            Assert.IsNotNull(healthCheck);
            var checkResult = await healthCheck.CheckHealthAsync(new());
            #endregion

            #region Assert
            Assert.IsNotNull(checkResult);
            Assert.AreEqual(HealthStatus.Healthy,checkResult.Status);
            Assert.AreEqual(Constants.HealthyDescription, checkResult.Description);
            Assert.AreEqual(pingResult.Host, checkResult.Data["Host"]);
            Assert.AreEqual(pingResult.Version, checkResult.Data["Version"]);
            Assert.AreEqual(pingResult.ResponseTime, checkResult.Data["ResponseTime"]);
            Assert.AreEqual(healthCheck,contractConnection.HealthCheck);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PingAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPingExceptionUnhealthyCheck()
        {
            #region Arrange
            var error = new Exception("Ping failed");

            var serviceConnection = new Mock<IPingableMessageServiceConnection>();
            serviceConnection.Setup(x => x.PingAsync())
                .ThrowsAsync(error);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var healthCheck = contractConnection.HealthCheck;
            Assert.IsNotNull(healthCheck);
            var checkResult = await healthCheck.CheckHealthAsync(new());
            #endregion

            #region Assert
            Assert.IsNotNull(checkResult);
            Assert.AreEqual(HealthStatus.Unhealthy, checkResult.Status);
            Assert.AreEqual(Constants.UnHealthyDescription, checkResult.Description);
            Assert.AreEqual(error, checkResult.Exception);
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

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var error = Assert.Throws<ArgumentOutOfRangeException>(() => _ = contractConnection.HealthCheck);
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            Assert.AreEqual("connection", error.ParamName);
            Assert.IsTrue(error.Message.StartsWith("Service connection is not Pingable"));
            #endregion

            #region Verify
            #endregion
        }
    }
}
