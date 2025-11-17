using Moq;
using MQContract.CQRS;
using MQContract.CQRS.Extensions;
using MQContract.Interfaces;

namespace CQRSTesting
{
    [TestClass]
    public class ICQRSBaseTests
    {
        [TestMethod]
        public async Task TestInvalidSourceConnection()
        {
            #region Arrange
            var mockConnection = new Mock<IContractConnection>();
            #endregion

            #region Act
            var error = Assert.Throws<InvalidConnectionException>(() => mockConnection.Object.CreateCQRSConnection());
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            Assert.IsInstanceOfType<InvalidConnectionException>(error);
            #endregion

            #region Verify
            #endregion
        }

        [TestMethod]
        public async Task TestConnectionDisposal()
        {
            #region Arrange
            var mockConnection = new Mock<IContractedConnection>();

            mockConnection.Setup(x => x.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            var cqrsConnection = mockConnection.Object.CreateCQRSConnection();
            #endregion

            #region Act
            await cqrsConnection.DisposeAsync();
            #endregion

            #region Assert 
            #endregion

            #region Verify
            mockConnection.Verify(x => x.DisposeAsync(), Times.Once);
            #endregion
        }
    }
}
