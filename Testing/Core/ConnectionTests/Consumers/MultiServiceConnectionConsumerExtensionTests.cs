using AutomatedTesting.Consumers;
using AutomatedTesting.Messages;
using Moq;
using MQContract.Extensions;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;

namespace CoreTesting.ConnectionTests.Consumers
{
    [TestClass]
    public class MultiServiceConnectionConsumerExtensionTests
    {
        private const string TestChannel = "TestConsumerChannel";
        private const string TestGroup = "TestConsumerGroup";
        private const bool ignoreMessageHeader = true;

        [TestMethod]
        public async Task TestRegisterPubSubConsumerAsyncWithInstanceExtension()
        {
            #region Arrange
            var mockConnection = new Mock<IMultiServiceContractConnection>();
            mockConnection.Setup(x => x.RegisterPubSubConsumerAsync(It.IsAny<IPubSubConsumer<BasicMessage>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<MessageFilters<BasicMessage>>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var messageFilters = new MessageFilters<BasicMessage>();
            var consumer = new BasicMessageConsumer();

            var connectionTask = new ValueTask<IMultiServiceContractConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterPubSubConsumerAsync(
                consumer,
                channel: TestChannel,
                group: TestGroup,
                ignoreMessageHeader: ignoreMessageHeader,
                messageFilters: messageFilters
, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterPubSubConsumerAsync(consumer, TestChannel, TestGroup,
                ignoreMessageHeader, messageFilters, It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterPubSubConsumerAsyncWithGenericExtension()
        {
            #region Arrange
            var mockConnection = new Mock<IMultiServiceContractConnection>();
            mockConnection.Setup(x => x.RegisterPubSubConsumerAsync<BasicMessage, BasicMessageConsumer>(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<MessageFilters<BasicMessage>>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var messageFilters = new MessageFilters<BasicMessage>();
            var consumer = new BasicMessageConsumer();

            var connectionTask = new ValueTask<IMultiServiceContractConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterPubSubConsumerAsync<BasicMessage, BasicMessageConsumer>(
                channel: TestChannel,
                group: TestGroup,
                ignoreMessageHeader: ignoreMessageHeader,
                messageFilters: messageFilters
, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterPubSubConsumerAsync<BasicMessage, BasicMessageConsumer>(TestChannel, TestGroup,
                ignoreMessageHeader, messageFilters, It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterPubSubConsumerAsyncWithTypeExtension()
        {
            #region Arrange
            var mockConnection = new Mock<IMultiServiceContractConnection>();
            mockConnection.Setup(x => x.RegisterPubSubConsumerAsync(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var consumer = new BasicMessageConsumer();

            var connectionTask = new ValueTask<IMultiServiceContractConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterPubSubConsumerAsync(
                typeof(BasicMessageConsumer),
                channel: TestChannel,
                group: TestGroup,
                ignoreMessageHeader: ignoreMessageHeader
, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterPubSubConsumerAsync(typeof(BasicMessageConsumer), TestChannel, TestGroup,
                ignoreMessageHeader, It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterPubSubAsyncConsumerAsyncWithInstanceExtension()
        {
            #region Arrange
            var mockConnection = new Mock<IMultiServiceContractConnection>();
            mockConnection.Setup(x => x.RegisterPubSubAsyncConsumerAsync(It.IsAny<IPubSubAsyncConsumer<BasicMessage>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<MessageFilters<BasicMessage>>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var messageFilters = new MessageFilters<BasicMessage>();
            var AsyncConsumer = new BasicMessageAsyncConsumer();

            var connectionTask = new ValueTask<IMultiServiceContractConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterPubSubAsyncConsumerAsync(
                AsyncConsumer,
                channel: TestChannel,
                group: TestGroup,
                ignoreMessageHeader: ignoreMessageHeader,
                messageFilters: messageFilters
, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterPubSubAsyncConsumerAsync(AsyncConsumer, TestChannel, TestGroup,
                ignoreMessageHeader, messageFilters, It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterPubSubAsyncConsumerAsyncWithGenericExtension()
        {
            #region Arrange
            var mockConnection = new Mock<IMultiServiceContractConnection>();
            mockConnection.Setup(x => x.RegisterPubSubAsyncConsumerAsync<BasicMessage, BasicMessageAsyncConsumer>(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<MessageFilters<BasicMessage>>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var messageFilters = new MessageFilters<BasicMessage>();
            var AsyncConsumer = new BasicMessageAsyncConsumer();

            var connectionTask = new ValueTask<IMultiServiceContractConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterPubSubAsyncConsumerAsync<BasicMessage, BasicMessageAsyncConsumer>(
                channel: TestChannel,
                group: TestGroup,
                ignoreMessageHeader: ignoreMessageHeader,
                messageFilters: messageFilters
, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterPubSubAsyncConsumerAsync<BasicMessage, BasicMessageAsyncConsumer>(TestChannel, TestGroup,
                ignoreMessageHeader, messageFilters, It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterPubSubAsyncConsumerAsyncWithTypeExtension()
        {
            #region Arrange
            var mockConnection = new Mock<IMultiServiceContractConnection>();
            mockConnection.Setup(x => x.RegisterPubSubAsyncConsumerAsync(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var AsyncConsumer = new BasicMessageAsyncConsumer();

            var connectionTask = new ValueTask<IMultiServiceContractConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterPubSubAsyncConsumerAsync(
                typeof(BasicMessageAsyncConsumer),
                channel: TestChannel,
                group: TestGroup,
                ignoreMessageHeader: ignoreMessageHeader
, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterPubSubAsyncConsumerAsync(typeof(BasicMessageAsyncConsumer), TestChannel, TestGroup,
                ignoreMessageHeader, It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterQueryResponseConsumerAsyncWithInstanceExtension()
        {
            #region Arrange
            var mockConnection = new Mock<IMultiServiceContractConnection>();
            mockConnection.Setup(x => x.RegisterQueryResponseConsumerAsync<BasicQueryMessage, BasicResponseMessage, IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>>
                (It.IsAny<IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<MessageFilters<BasicQueryMessage>>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var messageFilters = new MessageFilters<BasicQueryMessage>();
            var consumer = new BasicQueryConsumer();

            var connectionTask = new ValueTask<IMultiServiceContractConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask
                .RegisterQueryResponseConsumerAsync<BasicQueryMessage, BasicResponseMessage, BasicQueryConsumer>
            (
                consumer,
                channel: TestChannel,
                group: TestGroup,
                ignoreMessageHeader: ignoreMessageHeader,
                messageFilters: messageFilters
, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterQueryResponseConsumerAsync<BasicQueryMessage, BasicResponseMessage, IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>>(consumer, TestChannel, TestGroup,
                ignoreMessageHeader, messageFilters, It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterQueryResponseConsumerAsyncWithGenericExtension()
        {
            #region Arrange
            var mockConnection = new Mock<IMultiServiceContractConnection>();
            mockConnection.Setup(x => x.RegisterQueryResponseConsumerAsync<BasicQueryMessage, BasicResponseMessage, BasicQueryConsumer>(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<MessageFilters<BasicQueryMessage>>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var messageFilters = new MessageFilters<BasicQueryMessage>();
            var consumer = new BasicQueryConsumer();

            var connectionTask = new ValueTask<IMultiServiceContractConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterQueryResponseConsumerAsync<BasicQueryMessage, BasicResponseMessage, BasicQueryConsumer>(
                channel: TestChannel,
                group: TestGroup,
                ignoreMessageHeader: ignoreMessageHeader,
                messageFilters: messageFilters
, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterQueryResponseConsumerAsync<BasicQueryMessage, BasicResponseMessage, BasicQueryConsumer>(TestChannel, TestGroup,
                ignoreMessageHeader, messageFilters, It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterQueryResponseConsumerAsyncWithTypeExtension()
        {
            #region Arrange
            var mockConnection = new Mock<IMultiServiceContractConnection>();
            mockConnection.Setup(x => x.RegisterQueryResponseConsumerAsync(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var consumer = new BasicQueryConsumer();

            var connectionTask = new ValueTask<IMultiServiceContractConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterQueryResponseConsumerAsync(
                typeof(BasicQueryConsumer),
                channel: TestChannel,
                group: TestGroup,
                ignoreMessageHeader: ignoreMessageHeader
, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterQueryResponseConsumerAsync(typeof(BasicQueryConsumer), TestChannel, TestGroup,
                ignoreMessageHeader, It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterQueryResponseAsyncConsumerAsyncWithInstanceExtension()
        {
            #region Arrange
            var mockConnection = new Mock<IMultiServiceContractConnection>();
            mockConnection.Setup(x => x.RegisterQueryResponseAsyncConsumerAsync<BasicQueryMessage, BasicResponseMessage, IQueryResponseAsyncConsumer<BasicQueryMessage, BasicResponseMessage>>
                (It.IsAny<IQueryResponseAsyncConsumer<BasicQueryMessage, BasicResponseMessage>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<MessageFilters<BasicQueryMessage>>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var messageFilters = new MessageFilters<BasicQueryMessage>();
            var AsyncConsumer = new BasicQueryAsyncConsumer();

            var connectionTask = new ValueTask<IMultiServiceContractConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask
                .RegisterQueryResponseAsyncConsumerAsync<BasicQueryMessage, BasicResponseMessage, BasicQueryAsyncConsumer>
            (
                AsyncConsumer,
                channel: TestChannel,
                group: TestGroup,
                ignoreMessageHeader: ignoreMessageHeader,
                messageFilters: messageFilters
, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterQueryResponseAsyncConsumerAsync<BasicQueryMessage, BasicResponseMessage, IQueryResponseAsyncConsumer<BasicQueryMessage, BasicResponseMessage>>(AsyncConsumer, TestChannel, TestGroup,
                ignoreMessageHeader, messageFilters, It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterQueryResponseAsyncConsumerAsyncWithGenericExtension()
        {
            #region Arrange
            var mockConnection = new Mock<IMultiServiceContractConnection>();
            mockConnection.Setup(x => x.RegisterQueryResponseAsyncConsumerAsync<BasicQueryMessage, BasicResponseMessage, BasicQueryAsyncConsumer>(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<MessageFilters<BasicQueryMessage>>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var messageFilters = new MessageFilters<BasicQueryMessage>();
            var AsyncConsumer = new BasicQueryAsyncConsumer();

            var connectionTask = new ValueTask<IMultiServiceContractConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterQueryResponseAsyncConsumerAsync<BasicQueryMessage, BasicResponseMessage, BasicQueryAsyncConsumer>(
                channel: TestChannel,
                group: TestGroup,
                ignoreMessageHeader: ignoreMessageHeader,
                messageFilters: messageFilters
, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterQueryResponseAsyncConsumerAsync<BasicQueryMessage, BasicResponseMessage, BasicQueryAsyncConsumer>(TestChannel, TestGroup,
                ignoreMessageHeader, messageFilters, It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterQueryResponseAsyncConsumerAsyncWithTypeExtension()
        {
            #region Arrange
            var mockConnection = new Mock<IMultiServiceContractConnection>();
            mockConnection.Setup(x => x.RegisterQueryResponseAsyncConsumerAsync(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(mockConnection.Object));

            var AsyncConsumer = new BasicQueryAsyncConsumer();

            var connectionTask = new ValueTask<IMultiServiceContractConnection>(mockConnection.Object);
            #endregion

            #region Act
            var result = await connectionTask.RegisterQueryResponseAsyncConsumerAsync(
                typeof(BasicQueryAsyncConsumer),
                channel: TestChannel,
                group: TestGroup,
                ignoreMessageHeader: ignoreMessageHeader
, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.AreEqual(mockConnection.Object, result);
            #endregion

            #region Verify
            mockConnection.Verify(x => x.RegisterQueryResponseAsyncConsumerAsync(typeof(BasicQueryAsyncConsumer), TestChannel, TestGroup,
                ignoreMessageHeader, It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        public TestContext TestContext { get; set; }
    }
}
