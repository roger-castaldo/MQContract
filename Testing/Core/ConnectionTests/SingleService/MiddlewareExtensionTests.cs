using CoreTesting.ConnectionTests.Middlewares;
using CoreTesting.Messages;
using Moq;
using MQContract.Extensions;
using MQContract.Interfaces;
using MQContract.Interfaces.Middleware;

namespace CoreTesting.ConnectionTests.SingleService
{
    [TestClass]
    public class MiddlewareExtensionTests
    {
        [TestMethod]
        public async Task TestRegisterMiddlewareThroughGeneric()
        {
            #region Arrange
            var mockConnection = new Mock<IContractedConnection>();
            mockConnection.Setup(x => x.RegisterMiddlewareAsync<ChannelChangeMiddleware>())
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var connectionTask = new ValueTask<IContractedConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterMiddlewareAsync<ChannelChangeMiddleware>();
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterMiddlewareAsync<ChannelChangeMiddleware>(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterMiddlewareThroughGenericFunctionInstance()
        {
            #region Arrange
            var mockConnection = new Mock<IContractedConnection>();
            mockConnection.Setup(x => x.RegisterMiddlewareAsync<ChannelChangeMiddleware>(It.IsAny<Func<ChannelChangeMiddleware>>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var connectionTask = new ValueTask<IContractedConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterMiddlewareAsync<ChannelChangeMiddleware>(()=>new ChannelChangeMiddleware());
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterMiddlewareAsync<ChannelChangeMiddleware>(It.IsAny<Func<ChannelChangeMiddleware>>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterMiddlewareThroughType()
        {
            #region Arrange
            var mockConnection = new Mock<IContractedConnection>();
            mockConnection.Setup(x => x.RegisterMiddlewareAsync(It.IsAny<Type>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var connectionTask = new ValueTask<IContractedConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterMiddlewareAsync(typeof(ChannelChangeMiddleware));
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterMiddlewareAsync(typeof(ChannelChangeMiddleware)), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterMiddlewareThroughInstance()
        {
            #region Arrange
            var mockConnection = new Mock<IContractedConnection>();
            mockConnection.Setup(x => x.RegisterMiddlewareAsync(It.IsAny<IMiddleware>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var mockMiddleware = new Mock<IMiddleware>();

            var connectionTask = new ValueTask<IContractedConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterMiddlewareAsync(mockMiddleware.Object);
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterMiddlewareAsync(mockMiddleware.Object), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterMiddlewareThroughCallbackInstance()
        {
            #region Arrange
            var mockConnection = new Mock<IContractedConnection>();
            mockConnection.Setup(x => x.RegisterMiddlewareAsync(It.IsAny<Func<IMiddleware>>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var mockMiddleware = new Mock<IMiddleware>();

            var connectionTask = new ValueTask<IContractedConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterMiddlewareAsync(()=>mockMiddleware.Object);
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterMiddlewareAsync(It.IsAny<Func<IMiddleware>>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterMiddlewareThroughCallbackSpecificTypeInstance()
        {
            #region Arrange
            var mockConnection = new Mock<IContractedConnection>();
            mockConnection.Setup(x => x.RegisterMiddlewareAsync<BasicMessage>(It.IsAny<Func<ISpecificTypeMiddleware<BasicMessage>>>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var mockMiddleware = new Mock<ISpecificTypeMiddleware<BasicMessage>>();

            var connectionTask = new ValueTask<IContractedConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterMiddlewareAsync<BasicMessage>(() => mockMiddleware.Object);
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterMiddlewareAsync<BasicMessage>(It.IsAny<Func<ISpecificTypeMiddleware<BasicMessage>>>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterMiddlewareThroughSpecificTypeInstance()
        {
            #region Arrange
            var mockConnection = new Mock<IContractedConnection>();
            mockConnection.Setup(x => x.RegisterMiddlewareAsync<BasicMessage>(It.IsAny<ISpecificTypeMiddleware<BasicMessage>>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var mockMiddleware = new Mock<ISpecificTypeMiddleware<BasicMessage>>();

            var connectionTask = new ValueTask<IContractedConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterMiddlewareAsync<BasicMessage>(mockMiddleware.Object);
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterMiddlewareAsync<BasicMessage>(mockMiddleware.Object), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterMiddlewareThroughSpecificTypeGeneric()
        {
            #region Arrange
            var mockConnection = new Mock<IContractedConnection>();
            mockConnection.Setup(x => x.RegisterMiddlewareAsync<ChannelChangeMiddlewareForBasicMessage,BasicMessage>())
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var connectionTask = new ValueTask<IContractedConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterMiddlewareAsync<ChannelChangeMiddlewareForBasicMessage, BasicMessage>();
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterMiddlewareAsync<ChannelChangeMiddlewareForBasicMessage, BasicMessage>(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterMiddlewareThroughSpecificTypeGenericWithCallback()
        {
            #region Arrange
            var mockConnection = new Mock<IContractedConnection>();
            mockConnection.Setup(x => x.RegisterMiddlewareAsync<ChannelChangeMiddlewareForBasicMessage, BasicMessage>(It.IsAny<Func<ChannelChangeMiddlewareForBasicMessage>>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var connectionTask = new ValueTask<IContractedConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterMiddlewareAsync<ChannelChangeMiddlewareForBasicMessage, BasicMessage>(()=>new ChannelChangeMiddlewareForBasicMessage());
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterMiddlewareAsync<ChannelChangeMiddlewareForBasicMessage, BasicMessage>(It.IsAny<Func<ChannelChangeMiddlewareForBasicMessage>>()), Times.Once);
            #endregion
        }
    }
}
