using AutomatedTesting.Messages;
using Moq;
using MQContract;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Service;
using System.Diagnostics;
using System.Reflection;

namespace AutomatedTesting.ConnectionTests.SingleService
{
    [TestClass]
    public class SubscribeQueryResponseTests
    {
        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithNoExtendedAspects()
        {
            #region Arrange
            var acknowledged = false;

            var serviceSubscription = new Mock<IServiceSubscription>();

            var receivedActions = new List<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                Capture.In(receivedActions),
                Capture.In(errorActions),
                Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(async (ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message, acknowledge: () =>
                    {
                        acknowledged=true;
                        return ValueTask.CompletedTask;
                    });
                    serviceMessages.Add(rmessage);
                    var result = await receivedActions[0](rmessage);
                    return Helper.ProduceQueryResult(result);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);

            var message = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeQueryResponseWithNoExtendedAspects");
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                messages.Add(msg);
                return ValueTask.FromResult(new QueryResponseMessage<BasicResponseMessage>(responseMessage, null));
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage>(message);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, receivedActions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
            Assert.AreEqual(message, messages[0].Message);
            Assert.AreEqual(exception, exceptions[0]);
            Assert.IsFalse(result.IsError);
            Assert.IsNull(result.Error);
            Assert.AreEqual(result.Result, responseMessage);
            Assert.IsTrue(acknowledged);
            Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].ReceivedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithSpecificChannel()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var channels = new List<string>();
            var channelName = "TestSubscribeQueryResponseWithSpecificChannel";

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(),
                It.IsAny<Action<Exception>>(),
                Capture.In(channels),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var subscription1 = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>(
                (msg) => ValueTask.FromResult<QueryResponseMessage<BasicResponseMessage>>(null),
                (error) => { },
                channel: channelName);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var subscription2 = await contractConnection.SubscribeQueryAsyncResponseAsync<NoChannelMessage, BasicResponseMessage>(
                (msg) => ValueTask.FromResult<QueryResponseMessage<BasicResponseMessage>>(null),
                (error) => { },
                channel: channelName);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            #endregion

            #region Assert
            Assert.IsNotNull(subscription1);
            Assert.IsNotNull(subscription2);
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(channelName, channels[0]);
            Assert.AreEqual(channelName, channels[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithSpecificGroup()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var groups = new List<string>();
            var groupName = "TestSubscribeQueryResponseWithSpecificGroup";

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(),
                It.IsAny<Action<Exception>>(),
                It.IsAny<string>(),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var subscription1 = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>(
                (msg) => ValueTask.FromResult<QueryResponseMessage<BasicResponseMessage>>(null),
                (error) => { },
                group: groupName);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var subscription2 = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>(
                (msg) => ValueTask.FromResult<QueryResponseMessage<BasicResponseMessage>>(null),
                (error) => { });
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            #endregion

            #region Assert
            Assert.IsNotNull(subscription1);
            Assert.IsNotNull(subscription2);
            Assert.AreEqual(2, groups.Count);
            Assert.AreEqual(groupName, groups[0]);
            Assert.AreNotEqual(groupName, groups[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncNoMessageChannelThrowsError()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(),
                It.IsAny<Action<Exception>>(),
                It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var exception = await Assert.ThrowsExceptionAsync<MessageChannelNullException>(async () => await contractConnection.SubscribeQueryAsyncResponseAsync<NoChannelMessage, BasicResponseMessage>(
                (msg) => ValueTask.FromResult<QueryResponseMessage<BasicResponseMessage>>(null),
                (error) => { })
            );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual("message must have a channel value (Parameter 'channel')", exception.Message);
            Assert.AreEqual("channel", exception.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncReturnFailedSubscription()
        {
            #region Arrange
            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(),
                It.IsAny<Action<Exception>>(),
                It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IServiceSubscription?)null);
            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var exception = await Assert.ThrowsExceptionAsync<SubscriptionFailedException>(async () => await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>(
                (msg) => ValueTask.FromResult<QueryResponseMessage<BasicResponseMessage>>(null),
                (error) => { })
            );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncCleanup()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            serviceSubscription.Setup(x => x.EndAsync())
                .Returns(ValueTask.CompletedTask);

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(),
                It.IsAny<Action<Exception>>(),
                It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var subscription = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>(
                (msg) => ValueTask.FromResult<QueryResponseMessage<BasicResponseMessage>>(null),
                (error) => { }
            );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await subscription.EndAsync();
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithSynchronousActions()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var receivedActions = new List<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                Capture.In(receivedActions),
                Capture.In(errorActions),
                Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(async (ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    var result = await receivedActions[0](rmessage);
                    return Helper.ProduceQueryResult(result);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);

            var message1 = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects1");
            var message2 = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects2");
            var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeQueryResponseWithNoExtendedAspects");
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeQueryResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                messages.Add(msg);
                return new(responseMessage, null);
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.QueryAsync<BasicQueryMessage>(message1);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();
            var result2 = await contractConnection.QueryAsync<BasicQueryMessage>(message2);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result1);
            Assert.AreEqual(1, receivedActions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(2, serviceMessages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
            Assert.AreEqual(message1, messages[0].Message);
            Assert.AreEqual(message2, messages[1].Message);
            Assert.AreEqual(exception, exceptions[0]);
            Assert.IsFalse(result1.IsError);
            Assert.IsNull(result1.Error);
            Assert.AreEqual(result1.Result, responseMessage);
            Assert.IsFalse(result2.IsError);
            Assert.IsNull(result2.Error);
            Assert.AreEqual(result2.Result, responseMessage);
            Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].ReceivedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncErrorTriggeringInOurAction()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var receivedActions = new List<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                Capture.In(receivedActions),
                Capture.In(errorActions),
                Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(async (ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    var result = await receivedActions[0](rmessage);
                    return Helper.ProduceQueryResult(result);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);

            var message = new BasicQueryMessage("TestSubscribeQueryResponseAsyncErrorTriggeringInOurAction");
            var exception = new NullReferenceException("TestSubscribeQueryResponseAsyncErrorTriggeringInOurAction");
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeQueryResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                throw exception;
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage>(message);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(exceptions, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, receivedActions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(0, messages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(exception, exceptions[0]);
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(exception.Message, result.Error?.Message);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncEndAsync()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            serviceSubscription.Setup(x => x.EndAsync())
                .Returns(ValueTask.CompletedTask);

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeQueryResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                throw new NotImplementedException();
            }, (error) => { });
            await subscription.EndAsync();
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncAsyncCleanup()
        {
            #region Arrange
            var serviceSubscription = new Mock<IAsyncDisposable>();
            serviceSubscription.Setup(x => x.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.As<IServiceSubscription>().Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeQueryResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                throw new NotImplementedException();
            }, (error) => { });
            await subscription.DisposeAsync();
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.DisposeAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithNonAsyncCleanup()
        {
            #region Arrange
            var serviceSubscription = new Mock<IDisposable>();
            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.As<IServiceSubscription>().Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeQueryResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                throw new NotImplementedException();
            }, (error) => { });
            await subscription.DisposeAsync();
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.Dispose(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncSubscriptionsCleanup()
        {
            #region Arrange
            var serviceSubscription = new Mock<IDisposable>();

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.As<IServiceSubscription>().Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeQueryResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                throw new NotImplementedException();
            }, (error) => { });
            subscription.Dispose();
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.Dispose(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithThrowsConversionError()
        {
            #region Arrange
            var message = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects");

            var serviceSubscription = new Mock<IServiceSubscription>();
            var globalConverter = new Mock<IMessageEncoder>();
            globalConverter.Setup(x => x.DecodeAsync<BasicResponseMessage>(It.IsAny<Stream>()))
                .Returns(ValueTask.FromResult<BasicResponseMessage?>(null));
            globalConverter.Setup(x => x.EncodeAsync(It.IsAny<BasicResponseMessage>()))
                .Returns(ValueTask.FromResult<byte[]>([]));
            globalConverter.Setup(x => x.DecodeAsync<BasicQueryMessage>(It.IsAny<Stream>()))
                .Returns(ValueTask.FromResult<BasicQueryMessage?>(message));
            globalConverter.Setup(x => x.EncodeAsync(It.IsAny<BasicQueryMessage>()))
                .Returns(ValueTask.FromResult<byte[]>([]));

            var receivedActions = new List<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>();

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                Capture.In(receivedActions),
                It.IsAny<Action<Exception>>(),
                It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(async (ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message);
                    var result = await receivedActions[0](rmessage);
                    return Helper.ProduceQueryResult(result);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, defaultMessageEncoder: globalConverter.Object);


            var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();
            await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                messages.Add(msg);
                return ValueTask.FromResult(new QueryResponseMessage<BasicResponseMessage>(responseMessage, null));
            }, (error) => exceptions.Add(error), ignoreMessageHeader: true);
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage>(message);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsError);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Error?.Message));
            Assert.IsTrue(result.Error?.Message.Contains(typeof(BasicResponseMessage).FullName!));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task TestSubscribeQueryResponseAsyncWithTelemetryData(bool withLinking)
        {
            #region Arrange
            (var listener, var capturedActivities, var sourceName) = ConnectionHelper.SetupTelemetry();
            var acknowledged = false;
            ServiceQueryResult? queryResult = null;

            var serviceSubscription = new Mock<IServiceSubscription>();

            var receivedActions = new List<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                Capture.In(receivedActions),
                Capture.In(errorActions),
                Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(async (ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message, acknowledge: () =>
                    {
                        acknowledged=true;
                        return ValueTask.CompletedTask;
                    });
                    serviceMessages.Add(rmessage);
                    var result = await receivedActions[0](rmessage);
                    queryResult =  Helper.ProduceQueryResult(result);
                    return queryResult;
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: withLinking);

            var message = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeQueryResponseWithNoExtendedAspects");
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                messages.Add(msg);
                return ValueTask.FromResult(new QueryResponseMessage<BasicResponseMessage>(responseMessage, null));
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage>(message);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, receivedActions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
            Assert.AreEqual(message, messages[0].Message);
            Assert.AreEqual(exception, exceptions[0]);
            Assert.IsFalse(result.IsError);
            Assert.IsNull(result.Error);
            Assert.AreEqual(result.Result, responseMessage);
            Assert.IsTrue(acknowledged);
            Assert.AreEqual(4, capturedActivities.Count);
            ConnectionHelper.ValidateConsumeActivity<BasicQueryMessage>(
                serviceMessages[0],
                capturedActivities[1],
                "MQContract.ConsumeQueryMessage",
                serviceConnection.Object.GetType(),
                true,
                withLinking
            );
            ConnectionHelper.ValidatePublishActivity<BasicResponseMessage>(
                queryResult!,
                capturedActivities[2],
                "MQContract.ProduceQueryResponse",
                serviceConnection.Object.GetType(),
                true,
                withLinking
            );
            Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].ReceivedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }
    }
}
