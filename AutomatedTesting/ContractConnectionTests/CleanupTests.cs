using Moq;
using MQContract.Interfaces.Service;
using MQContract;

namespace AutomatedTesting.ContractConnectionTests
{
    [TestClass]
    public class CleanupTests
    {
        [TestMethod]
        public async Task TestCloseAsync()
        {
            #region Arrange
            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.CloseAsync())
                .Returns(ValueTask.CompletedTask);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.CloseAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public void TestDispose()
        {
            #region Arrange
            var serviceConnection = new Mock<IDisposable>();

            var contractConnection = ContractConnection.Instance(serviceConnection.As<IMessageServiceConnection>().Object);
            #endregion

            #region Act
            contractConnection.Dispose();
            #endregion

            #region Assert
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.Dispose(), Times.Once);
            #endregion
        }

        [TestMethod]
        public void TestDisposeWithAsyncDispose()
        {
            #region Arrange
            var serviceConnection = new Mock<IAsyncDisposable>();
            serviceConnection.Setup(x=>x.DisposeAsync()).Returns(ValueTask.CompletedTask);

            var contractConnection = ContractConnection.Instance(serviceConnection.As<IMessageServiceConnection>().Object);
            #endregion

            #region Act
            contractConnection.Dispose();
            #endregion

            #region Assert
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.DisposeAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestDisposeAsyncWithDispose()
        {
            #region Arrange
            var serviceConnection = new Mock<IDisposable>();

            var contractConnection = ContractConnection.Instance(serviceConnection.As<IMessageServiceConnection>().Object);
            #endregion

            #region Act
            await contractConnection.DisposeAsync();
            #endregion

            #region Assert
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.Dispose(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestDisposeAsyncWithDisposeAsync()
        {
            #region Arrange
            var serviceConnection = new Mock<IAsyncDisposable>();

            var contractConnection = ContractConnection.Instance(serviceConnection.As<IMessageServiceConnection>().Object);
            #endregion

            #region Act
            await contractConnection.DisposeAsync();
            #endregion

            #region Assert
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.DisposeAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestDisposeAsyncWithDisposeAsyncAndDispose()
        {
            #region Arrange
            var serviceConnection = new Mock<IAsyncDisposable>();

            var contractConnection = ContractConnection.Instance(serviceConnection.As<IDisposable>().As<IMessageServiceConnection>().Object);
            #endregion

            #region Act
            await contractConnection.DisposeAsync();
            #endregion

            #region Assert
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.DisposeAsync(), Times.Once);
            serviceConnection.As<IDisposable>().Verify(x => x.Dispose(), Times.Never);
            #endregion
        }
    }
}
