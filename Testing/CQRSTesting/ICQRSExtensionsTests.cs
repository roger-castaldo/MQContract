using CQRSTesting.Messages;
using Moq;
using MQContract.CQRS.Extensions;
using MQContract.CQRS.Interfaces;
using MQContract.CQRS.Interfaces.Command;
using MQContract.CQRS.Interfaces.Query;

namespace CQRSTesting
{
    [TestClass]
    public class ICQRSExtensionsTests
    {
        private const string Group = "TestMockGroup";

        [TestMethod]
        public async Task TestRegisterCommandProcessorAsync()
        {
            #region Arrange
            var mockConnection = new Mock<ICQRSConnection>();
            mockConnection.Setup(x => x.RegisterCommandProcessorAsync(It.IsAny<ICommandProcessor<BasicCommand>>(), It.IsAny<string>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var mockProcessor = new Mock<ICommandProcessor<BasicCommand>>();

            var connectionTask = new ValueTask<ICQRSConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask
                .RegisterCommandProcessorAsync<BasicCommand>(mockProcessor.Object, group: Group);

            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterCommandProcessorAsync(mockProcessor.Object, Group),
                Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterCommandProcessorWithReturnAsync()
        {
            #region Arrange
            var mockConnection = new Mock<ICQRSConnection>();
            mockConnection.Setup(x => x.RegisterCommandProcessorAsync(It.IsAny<ICommandProcessor<BasicResponseCommand, BasicCommandResponse>>(), It.IsAny<string>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var mockProcessor = new Mock<ICommandProcessor<BasicResponseCommand, BasicCommandResponse>>();

            var connectionTask = new ValueTask<ICQRSConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask
                .RegisterCommandProcessorAsync<BasicResponseCommand, BasicCommandResponse>(mockProcessor.Object, group: Group);

            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterCommandProcessorAsync(mockProcessor.Object, Group),
                Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterQueryProcessorWithReturnAsync()
        {
            #region Arrange
            var mockConnection = new Mock<ICQRSConnection>();
            mockConnection.Setup(x => x.RegisterQueryProcessorAsync(It.IsAny<IQueryProcessor<BasicQuery, BasicQueryResponse>>(), It.IsAny<string>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var mockProcessor = new Mock<IQueryProcessor<BasicQuery, BasicQueryResponse>>();

            var connectionTask = new ValueTask<ICQRSConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask
                .RegisterQueryProcessorAsync<BasicQuery, BasicQueryResponse>(mockProcessor.Object, group: Group);

            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterQueryProcessorAsync(mockProcessor.Object, Group),
                Times.Once);
            #endregion
        }
    }
}
