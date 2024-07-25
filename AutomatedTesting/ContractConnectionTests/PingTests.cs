using Moq;
using MQContract;
using MQContract.Interfaces.Service;

namespace AutomatedTesting.ContractConnectionTests
{
    [TestClass]
    public class PingTests
    {
        [TestMethod]
        public async Task TestPingAsync()
        {
            #region Arrange
            var pingResult = new Mock<IPingResult>();
            pingResult.Setup(x => x.Host)
                .Returns("TestHost");
            pingResult.Setup(x => x.Version)
                .Returns("1.0.0");
            pingResult.Setup(x => x.ResponseTime)
                .Returns(TimeSpan.FromSeconds(5));

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PingAsync())
                .Returns(Task.FromResult<IPingResult>(pingResult.Object));

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var result = await contractConnection.PingAsync();
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(pingResult.Object.Host, result.Host);
            Assert.AreEqual(pingResult.Object.ResponseTime, result.ResponseTime);
            Assert.AreEqual(pingResult.Object.Version, result.Version);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PingAsync(), Times.Once);
            #endregion
        }
    }
}
