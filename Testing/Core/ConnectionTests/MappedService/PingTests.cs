using Moq;
using MQContract;
using MQContract.Interfaces.Service;

namespace CoreTesting.ConnectionTests.MappedService
{
    [TestClass]
    public class PingTests
    {
        private const string ServiceName = "testService";

        [TestMethod]
        public async Task TestPingAsync()
        {
            #region Arrange
            var pingResult = new PingResult("TestHost", "1.0.0", TimeSpan.FromSeconds(5));

            var serviceConnection = new Mock<IPingableMessageServiceConnection>();
            serviceConnection.Setup(x => x.PingAsync())
                .ReturnsAsync(pingResult);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var result = await contractConnection.PingAsync();
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(pingResult, result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PingAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPingAsyncWithNoPingableService()
        {
            #region Arrange
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var error = await Assert.ThrowsExactlyAsync<PingNotSupportedException>(async () => await contractConnection.PingAsync());
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            #endregion

            #region Verify
            #endregion
        }

        [TestMethod]
        public async Task TestPingAsyncWithMultiplePingableServicesError()
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
            var error = await Assert.ThrowsExactlyAsync<TooManyConnectionMatchesException>(async () => await contractConnection.PingAsync());
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PingAsync(), Times.Never);
            #endregion
        }
    }
}
