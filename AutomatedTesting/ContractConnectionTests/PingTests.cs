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
            var pingResult = new PingResult("TestHost", "1.0.0", TimeSpan.FromSeconds(5));
            
            var serviceConnection = new Mock<IPingableMessageServiceConnection>();
            serviceConnection.Setup(x => x.PingAsync())
                .ReturnsAsync(pingResult);

            var contractConnection = new ContractConnection(serviceConnection.Object);
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
    }
}
