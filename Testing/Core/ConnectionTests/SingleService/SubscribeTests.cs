using CoreTesting.Messages;
using Moq;
using MQContract;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Service;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace CoreTesting.ConnectionTests.SingleService
{
    [TestClass]
    public class SubscribeTests
    {
        [TestMethod]
        public async Task TestSubscribeAsyncWithNoExtendedAspects()
        {
            #region Arrange
            var acknowledged = false;

            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message, acknowledge: () =>
                    {
                        acknowledged=true;
                        return ValueTask.CompletedTask;
                    });
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<BasicMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>((msg) =>
            {
                messages.Add(msg);
                return ValueTask.CompletedTask;
            }, (error) => exceptions.Add(error), cancellationToken: TestContext.CancellationToken);
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(new TransmissionMessage<BasicMessage>(message), cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.HasCount(1, actions);
            Assert.HasCount(1, channels);
            Assert.HasCount(1, groups);
            Assert.HasCount(1, serviceMessages);
            Assert.HasCount(1, errorActions);
            Assert.HasCount(1, exceptions);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.HasCount(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys);
            Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
            Assert.AreEqual(message, messages[0].Message);
            Assert.AreEqual(exception, exceptions[0]);
            Assert.IsTrue(acknowledged);
            Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].ReceivedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithCompressionDueToMessageSize()
        {
            #region Arrange
            var acknowledged = false;

            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message, acknowledge: () =>
                    {
                        acknowledged=true;
                        return ValueTask.CompletedTask;
                    });
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });
            serviceConnection.Setup(x => x.MaxMessageBodySize)
                .Returns(35);

            var message = new BasicMessage("AAAAAAAAAAAAAAAAAAAaaaaaaaaaaaaaaaaaaaa");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<BasicMessage>>();
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>((msg) =>
            {
                messages.Add(msg);
                return ValueTask.CompletedTask;
            }, (error) => { }, cancellationToken: TestContext.CancellationToken);
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(new TransmissionMessage<BasicMessage>(message), cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.HasCount(1, actions);
            Assert.HasCount(1, channels);
            Assert.HasCount(1, groups);
            Assert.HasCount(1, serviceMessages);
            Assert.HasCount(1, errorActions);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.HasCount(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys);
            Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
            Assert.AreEqual(message, messages[0].Message);
            Assert.IsTrue(acknowledged);
            Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].ReceivedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithSpecificChannel()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var channels = new List<string>();
            var channelName = "TestSubscribeAsyncWithSpecificChannel";

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), Capture.In(channels),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var subscription1 = await contractConnection.SubscribeAsync<BasicMessage>((msg) => ValueTask.CompletedTask, (error) => { }, channel: channelName, cancellationToken: TestContext.CancellationToken);
            var subscription2 = await contractConnection.SubscribeAsync<NoChannelMessage>((msg) => ValueTask.CompletedTask, (error) => { }, channel: channelName, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription1);
            Assert.IsNotNull(subscription2);
            Assert.HasCount(2, channels);
            Assert.AreEqual(channelName, channels[0]);
            Assert.AreEqual(channelName, channels[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithSpecificGroup()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var groups = new List<string>();
            var groupName = "TestSubscribeAsyncWithSpecificGroup";

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var subscription1 = await contractConnection.SubscribeAsync<BasicMessage>((msg) => ValueTask.CompletedTask, (error) => { }, group: groupName, cancellationToken: TestContext.CancellationToken);
            var subscription2 = await contractConnection.SubscribeAsync<BasicMessage>((msg) => ValueTask.CompletedTask, (error) => { }, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription1);
            Assert.IsNotNull(subscription2);
            Assert.HasCount(2, groups);
            Assert.AreEqual(groupName, groups[0]);
            Assert.AreNotEqual(groupName, groups[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncNoMessageChannelThrowsError()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var exception = await Assert.ThrowsExactlyAsync<MessageChannelNullException>(async () => await contractConnection.SubscribeAsync<NoChannelMessage>((msg) => ValueTask.CompletedTask, (error) => { }, cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual("message must have a channel value (Parameter 'channel')", exception.Message);
            Assert.AreEqual("channel", exception.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncReturnFailedSubscription()
        {
            #region Arrange
            var serviceConnection = new Mock<IMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IServiceSubscription?)null);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var exception = await Assert.ThrowsExactlyAsync<SubscriptionFailedException>(async () => await contractConnection.SubscribeAsync<BasicMessage>(msg => ValueTask.CompletedTask, err => { }, cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }


        [TestMethod]
        public async Task TestSubscriptionsEndAsync()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            serviceSubscription.Setup(x => x.EndAsync())
                .Returns(ValueTask.CompletedTask);

            var serviceConnection = new Mock<IMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>(msg => ValueTask.CompletedTask, err => { }, cancellationToken: TestContext.CancellationToken);
            await subscription.EndAsync();
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncCleanup()
        {
            #region Arrange
            var serviceSubscription = new Mock<IAsyncDisposable>();
            serviceSubscription.Setup(x => x.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            var serviceConnection = new Mock<IMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.As<IServiceSubscription>().Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>(msg => ValueTask.CompletedTask, err => { }, cancellationToken: TestContext.CancellationToken);
            await subscription.DisposeAsync();
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.DisposeAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithNonAsyncCleanup()
        {
            #region Arrange
            var serviceSubscription = new Mock<IDisposable>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.As<IServiceSubscription>().Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>(msg => ValueTask.CompletedTask, err => { }, cancellationToken: TestContext.CancellationToken);
            await subscription.DisposeAsync();
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.Dispose(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscriptionsCleanup()
        {
            #region Arrange
            var serviceSubscription = new Mock<IDisposable>();

            var serviceConnection = new Mock<IMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.As<IServiceSubscription>().Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>(msg => ValueTask.CompletedTask, err => { }, cancellationToken: TestContext.CancellationToken);
            subscription.Dispose();
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.Dispose(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithSynchronousActions()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var message1 = new BasicMessage("TestSubscribeAsyncWithSynchronousActions1");
            var message2 = new BasicMessage("TestSubscribeAsyncWithSynchronousActions2");
            var exception = new NullReferenceException("TestSubscribeAsyncWithSynchronousActions");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<BasicMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>((msg) =>
            {
                messages.Add(msg);
            }, (error) => exceptions.Add(error), cancellationToken: TestContext.CancellationToken);
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.PublishAsync<BasicMessage>(new TransmissionMessage<BasicMessage>(message1), cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();
            var result2 = await contractConnection.PublishAsync<BasicMessage>(new TransmissionMessage<BasicMessage>(message2), cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.HasCount(1, actions);
            Assert.HasCount(1, channels);
            Assert.HasCount(1, groups);
            Assert.HasCount(2, serviceMessages);
            Assert.HasCount(1, errorActions);
            Assert.HasCount(1, exceptions);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.HasCount(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys);
            Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
            Assert.AreEqual(message1, messages[0].Message);
            Assert.AreEqual(message2, messages[1].Message);
            Assert.AreEqual(exception, exceptions[0]);
            Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].ReceivedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithErrorTriggeringInOurAction()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<BasicMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>((msg) =>
            {
                throw exception;
            }, (error) => exceptions.Add(error), cancellationToken: TestContext.CancellationToken);
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(new TransmissionMessage<BasicMessage>(message), cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(exceptions, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.HasCount(1, actions);
            Assert.HasCount(1, channels);
            Assert.HasCount(1, groups);
            Assert.HasCount(1, serviceMessages);
            Assert.IsEmpty(messages);
            Assert.HasCount(1, errorActions);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(exception, exceptions[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithCorruptMetaDataHeaderException()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message, $"{message.MessageTypeID}:XXXX");
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<BasicMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>((msg) => { return ValueTask.CompletedTask; }, (error) => exceptions.Add(error), cancellationToken: TestContext.CancellationToken);
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(new TransmissionMessage<BasicMessage>(message), cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(exceptions, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.HasCount(1, actions);
            Assert.HasCount(1, channels);
            Assert.HasCount(1, groups);
            Assert.HasCount(1, serviceMessages);
            Assert.IsEmpty(messages);
            Assert.HasCount(1, errorActions);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.IsInstanceOfType<InvalidCastException>(exceptions[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithDisposal()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<BasicMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>((msg) =>
            {
                messages.Add(msg);
                return ValueTask.CompletedTask;
            }, (error) => exceptions.Add(error), cancellationToken: TestContext.CancellationToken);
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(new TransmissionMessage<BasicMessage>(message), cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.HasCount(1, actions);
            Assert.HasCount(1, channels);
            Assert.HasCount(1, groups);
            Assert.HasCount(1, serviceMessages);
            Assert.HasCount(1, errorActions);
            Assert.HasCount(1, exceptions);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.HasCount(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys);
            Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
            Assert.AreEqual(message, messages[0].Message);
            Assert.AreEqual(exception, exceptions[0]);
            Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].ReceivedTimestamp).TotalMilliseconds}ms");
            Exception? disposeError = null;
            try
            {
                await subscription.EndAsync();
            }
            catch (Exception e)
            {
                disposeError = e;
            }
            Assert.IsNull(disposeError);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithSingleConversion()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<NamedAndVersionedMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<NamedAndVersionedMessage>((msg) =>
            {
                messages.Add(msg);
                return ValueTask.CompletedTask;
            }, (error) => exceptions.Add(error), cancellationToken: TestContext.CancellationToken);
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(new TransmissionMessage<BasicMessage>(message), channel: typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.HasCount(1, actions);
            Assert.HasCount(1, channels);
            Assert.HasCount(1, groups);
            Assert.HasCount(1, serviceMessages);
            Assert.HasCount(1, errorActions);
            Assert.HasCount(1, exceptions);
            Assert.AreEqual(typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.HasCount(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys);
            Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
            Assert.AreEqual(message.Name, messages[0].Message.TestName);
            Assert.AreEqual(exception, exceptions[0]);
            Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].ReceivedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithMultipleStepConversion()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var message = new NoChannelMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<NamedAndVersionedMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<NamedAndVersionedMessage>((msg) =>
            {
                messages.Add(msg);
                return ValueTask.CompletedTask;
            }, (error) => exceptions.Add(error), cancellationToken: TestContext.CancellationToken);
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<NoChannelMessage>(new TransmissionMessage<NoChannelMessage>(message), channel: typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.HasCount(1, actions);
            Assert.HasCount(1, channels);
            Assert.HasCount(1, groups);
            Assert.HasCount(1, serviceMessages);
            Assert.HasCount(1, errorActions);
            Assert.HasCount(1, exceptions);
            Assert.AreEqual(typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.HasCount(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys);
            Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
            Assert.AreEqual(message.TestName, messages[0].Message.TestName);
            Assert.AreEqual(exception, exceptions[0]);
            Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].ReceivedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithConversionAndGlobalEncoder()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();
            var globalEncoder = new Mock<IMessageEncoder>();

            var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });
            globalEncoder.Setup(x => x.DecodeAsync<BasicMessage>(It.IsAny<Stream>()))
                .Returns(async (Stream stream) => await JsonSerializer.DeserializeAsync<BasicMessage>(stream, cancellationToken: TestContext.CancellationToken));
            globalEncoder.Setup(x => x.EncodeAsync<BasicMessage>(It.IsAny<BasicMessage>()))
                .Returns((BasicMessage message) => ValueTask.FromResult(JsonSerializer.SerializeToUtf8Bytes(message)));

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, defaultMessageEncoder: globalEncoder.Object);
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<NamedAndVersionedMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<NamedAndVersionedMessage>((msg) =>
            {
                messages.Add(msg);
                return ValueTask.CompletedTask;
            }, (error) => exceptions.Add(error), cancellationToken: TestContext.CancellationToken);
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(new TransmissionMessage<BasicMessage>(message), channel: typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.HasCount(1, actions);
            Assert.HasCount(1, channels);
            Assert.HasCount(1, groups);
            Assert.HasCount(1, serviceMessages);
            Assert.HasCount(1, errorActions);
            Assert.HasCount(1, exceptions);
            Assert.AreEqual(typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.HasCount(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys);
            Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
            Assert.AreEqual(message.Name, messages[0].Message.TestName);
            Assert.AreEqual(exception, exceptions[0]);
            Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].ReceivedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            globalEncoder.Verify(x => x.DecodeAsync<BasicMessage>(It.IsAny<Stream>()), Times.Once);
            globalEncoder.Verify(x => x.EncodeAsync<BasicMessage>(It.IsAny<BasicMessage>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithNoConversionPath()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var message = new BasicQueryMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<NamedAndVersionedMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<NamedAndVersionedMessage>((msg) =>
            {
                messages.Add(msg);
                return ValueTask.CompletedTask;
            }, (error) => exceptions.Add(error), cancellationToken: TestContext.CancellationToken);
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicQueryMessage>(new TransmissionMessage<BasicQueryMessage>(message), channel: typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(exceptions, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.HasCount(1, actions);
            Assert.HasCount(1, channels);
            Assert.HasCount(1, groups);
            Assert.IsEmpty(messages);
            Assert.HasCount(1, errorActions);
            Assert.AreEqual(typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.HasCount(1, exceptions.OfType<InvalidCastException>());
            Assert.Contains(exception, exceptions);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task TestSubscribeAsyncWithTelemetryData(bool withLinking)
        {
            #region Arrange
            (var listener, var capturedActivities, var sourceName) = ConnectionHelper.SetupTelemetry();
            var acknowledged = false;

            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<ReceivedServiceMessage>();
            var publishedMessages = new List<ServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(publishedMessages), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message, acknowledge: () =>
                    {
                        acknowledged=true;
                        return ValueTask.CompletedTask;
                    });
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: withLinking);
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<BasicMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>((msg) =>
            {
                messages.Add(msg);
                return ValueTask.CompletedTask;
            }, (error) => exceptions.Add(error), cancellationToken: TestContext.CancellationToken);
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(new TransmissionMessage<BasicMessage>(message), cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.HasCount(1, actions);
            Assert.HasCount(1, channels);
            Assert.HasCount(1, groups);
            Assert.HasCount(1, serviceMessages);
            Assert.HasCount(1, errorActions);
            Assert.HasCount(1, exceptions);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.HasCount(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys);
            Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
            Assert.AreEqual(message, messages[0].Message);
            Assert.AreEqual(exception, exceptions[0]);
            Assert.IsTrue(acknowledged);
            Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].ReceivedTimestamp).TotalMilliseconds}ms");
            Assert.HasCount(2, capturedActivities);
            ConnectionHelper.ValidatePublishActivity<BasicMessage>(
                publishedMessages[0],
                capturedActivities[0],
                "MQContract.PublishMessage",
                serviceConnection.Object.GetType(),
                true,
                withLinking
            );
            ConnectionHelper.ValidateConsumeActivity<BasicMessage>(
                serviceMessages[0],
                capturedActivities[1],
                "MQContract.ConsumeMessage",
                serviceConnection.Object.GetType(),
                true,
                withLinking
            );
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        [DataRow("headerValue", "differentHeaderValue", "messageHeaderValue", "messageHeaderValue", "messageValue", "messageValue", true)]
        [DataRow("headerValue", "differentHeaderValue", "messageHeaderValue", "messageHeaderValue", "messageValue", "messageValue", false)]
        [DataRow("headerValue", "headerValue", "messageHeaderValue", "differentMessageHeaderValue", "messageValue", "messageValue", true)]
        [DataRow("headerValue", "headerValue", "messageHeaderValue", "differentMessageHeaderValue", "messageValue", "messageValue", false)]
        [DataRow("headerValue", "headerValue", "messageHeaderValue", "messageHeaderValue", "messageValue", "differentMessageValue", true)]
        [DataRow("headerValue", "headerValue", "messageHeaderValue", "messageHeaderValue", "messageValue", "differentMessageValue", false)]
        [DataRow("headerValue", "headerValue", "messageHeaderValue", "messageHeaderValue", "messageValue", "messageValue", true)]
        public async Task TestSubscribeAsyncWithFiltering(string headerValue, string checkValue, string messageHeaderValue, string messageHeaderCheckValue,
            string messageValue, string messageCheckValue, bool acknowledgeDrop)
        {
            #region Arrange
            var headerKey = "testHeader";
            var messageHeaderKey = "testMessageHeader";
            var acknowledged = false;

            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message, acknowledge: () =>
                    {
                        acknowledged=true;
                        return ValueTask.CompletedTask;
                    });
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var message = new BasicMessage(messageValue);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<BasicMessage>>();
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>((msg) =>
            {
                messages.Add(msg);
                return ValueTask.CompletedTask;
            }, (error) => { },
            messageFilters: new(
                HeaderFilter: (header) =>
                    ValueTask.FromResult<MessageFilterResult>((Equals(header[headerKey], checkValue), acknowledgeDrop) switch
                    {
                        (true, _) => MessageFilterResult.Allow,
                        (false, false) => MessageFilterResult.DropAndDontAcknowledge,
                        (false, true) => MessageFilterResult.DropAndAcknowledge
                    }),
                MessageFilter: (serviceMessage, header) =>
                    ValueTask.FromResult<MessageFilterResult>((Equals(header[messageHeaderKey], messageHeaderCheckValue), Equals(serviceMessage.Name, messageCheckValue), acknowledgeDrop) switch
                    {
                        (true, true, _) => MessageFilterResult.Allow,
                        (false, _, false) => MessageFilterResult.DropAndDontAcknowledge,
                        (false, _, true) => MessageFilterResult.DropAndAcknowledge,
                        (_, false, false) => MessageFilterResult.DropAndDontAcknowledge,
                        (_, false, true) => MessageFilterResult.DropAndAcknowledge,
                    })
            ), cancellationToken: TestContext.CancellationToken);
            var result = await contractConnection.PublishAsync<BasicMessage>(new TransmissionMessage<BasicMessage>(message, Header: new([
                new KeyValuePair<string,string?>(headerKey,headerValue),
                new KeyValuePair<string,string?>(messageHeaderKey,messageHeaderValue)
            ])), cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.HasCount(1, actions);
            Assert.HasCount(1, serviceMessages);
            if (Equals(headerValue, checkValue) && Equals(messageHeaderValue, messageHeaderCheckValue) && Equals(messageValue, messageCheckValue))
            {
                Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
                Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
                Assert.HasCount(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys);
                Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
                Assert.AreEqual(message, messages[0].Message);
            }
            else
            {
                Assert.IsEmpty(messages);
            }
            Assert.AreEqual(acknowledgeDrop, acknowledged);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        public TestContext TestContext { get; set; }
    }
}

