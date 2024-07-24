using AutomatedTesting.Messages;
using Moq;
using MQContract.Attributes;
using MQContract.Interfaces.Service;
using MQContract.Interfaces;
using MQContract;
using System.Diagnostics;
using System.Reflection;

namespace AutomatedTesting.ContractConnectionTests
{
    [TestClass]
    public class SubscribeQueryResponseTests
    {
        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithNoExtendedAspects()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var recievedActions = new List<Func<IRecievedServiceMessage, Task<IServiceMessage>>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<IRecievedServiceMessage>();

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                Capture.In<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(recievedActions),
                Capture.In<Action<Exception>>(errorActions),
                Capture.In<string>(channels),
                Capture.In<string>(groups),
                It.IsAny<IServiceChannelOptions>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IServiceSubscription>(serviceSubscription.Object));
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(async (IServiceMessage message, TimeSpan timeout, IServiceChannelOptions options, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceRecievedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    var result = await recievedActions[0](rmessage);
                    return Helper.ProduceQueryResult(result);
                });

            var contractConnection = new ContractConnection(serviceConnection.Object);

            var message = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeQueryResponseWithNoExtendedAspects");
            #endregion

            #region Act
            var messages = new List<IMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeQueryResponseAsync<BasicQueryMessage,BasicResponseMessage>((msg) => {
                messages.Add(msg);
                return Task.FromResult(new QueryResponseMessage<BasicResponseMessage>(responseMessage,null));
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage>(message);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<IMessage<BasicQueryMessage>>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, recievedActions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsFalse(string.IsNullOrWhiteSpace(groups[0]));
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(serviceMessages[0].RecievedTimestamp, messages[0].RecievedTimestamp);
            Assert.AreEqual(message, messages[0].Message);
            Assert.AreEqual(exception, exceptions[0]);
            Assert.IsFalse(result.IsError);
            Assert.IsNull(result.Error);
            Assert.AreEqual(result.Result, responseMessage);
            System.Diagnostics.Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].RecievedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithSpecificChannel()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var channels = new List<string>();
            var channelName = "TestSubscribeQueryResponseWithSpecificChannel";

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                It.IsAny<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(),
                It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels),
                It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IServiceSubscription>(serviceSubscription.Object));
            
            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var subscription1 = await contractConnection.SubscribeQueryResponseAsync<BasicQueryMessage, BasicResponseMessage>(
                (msg) => Task.FromResult<QueryResponseMessage<BasicResponseMessage>>(null), 
                (error) => { },
                channel:channelName);
            var subscription2 = await contractConnection.SubscribeQueryResponseAsync<NoChannelMessage, BasicResponseMessage>(
                (msg) => Task.FromResult<QueryResponseMessage<BasicResponseMessage>>(null),
                (error) => { },
                channel: channelName);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription1);
            Assert.IsNotNull(subscription2);
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(channelName, channels[0]);
            Assert.AreEqual(channelName, channels[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithSpecificGroup()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var groups = new List<string>();
            var groupName = "TestSubscribeQueryResponseWithSpecificGroup";

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                It.IsAny<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(),
                It.IsAny<Action<Exception>>(),
                It.IsAny<string>(),
                Capture.In<string>(groups),
                It.IsAny<IServiceChannelOptions>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IServiceSubscription>(serviceSubscription.Object));
            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var subscription1 = await contractConnection.SubscribeQueryResponseAsync<BasicQueryMessage, BasicResponseMessage>(
                (msg) => Task.FromResult<QueryResponseMessage<BasicResponseMessage>>(null),
                (error) => { },
                group: groupName);
            var subscription2 = await contractConnection.SubscribeQueryResponseAsync<BasicQueryMessage, BasicResponseMessage>(
                (msg) => Task.FromResult<QueryResponseMessage<BasicResponseMessage>>(null),
                (error) => { });
            #endregion

            #region Assert
            Assert.IsNotNull(subscription1);
            Assert.IsNotNull(subscription2);
            Assert.AreEqual(2, groups.Count);
            Assert.AreEqual(groupName, groups[0]);
            Assert.AreNotEqual(groupName, groups[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithServiceChannelOptions()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var serviceChannelOptions = new TestServiceChannelOptions("TestSubscribeQueryResponseWithServiceChannelOptions");
            List<IServiceChannelOptions> options = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                It.IsAny<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(),
                It.IsAny<Action<Exception>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                Capture.In<IServiceChannelOptions>(options),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IServiceSubscription>(serviceSubscription.Object));
            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeQueryResponseAsync<BasicQueryMessage, BasicResponseMessage>(
                (msg) => Task.FromResult<QueryResponseMessage<BasicResponseMessage>>(null),
                (error) => { },
                options:serviceChannelOptions);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            Assert.AreEqual(1, options.Count);
            Assert.IsInstanceOfType<TestServiceChannelOptions>(options[0]);
            Assert.AreEqual(serviceChannelOptions, options[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncNoMessageChannelThrowsError()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                It.IsAny<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(),
                It.IsAny<Action<Exception>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IServiceSubscription>(serviceSubscription.Object));
            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var exception = await Assert.ThrowsExceptionAsync<MessageChannelNullException>(() => contractConnection.SubscribeQueryResponseAsync<NoChannelMessage, BasicResponseMessage>(
                (msg) => Task.FromResult<QueryResponseMessage<BasicResponseMessage>>(null),
                (error) => { })
            );
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual("message must have a channel value (Parameter 'channel')", exception.Message);
            Assert.AreEqual("channel", exception.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncReturnFailedSubscription()
        {
            #region Arrange
            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                It.IsAny<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(),
                It.IsAny<Action<Exception>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IServiceSubscription?>(null));
            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var exception = await Assert.ThrowsExceptionAsync<SubscriptionFailedException>(() => contractConnection.SubscribeQueryResponseAsync<BasicQueryMessage, BasicResponseMessage>(
                (msg) => Task.FromResult<QueryResponseMessage<BasicResponseMessage>>(null),
                (error) => { })
            );
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncCleanup()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            serviceSubscription.Setup(x => x.EndAsync())
                .Returns(Task.CompletedTask);

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                It.IsAny<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(),
                It.IsAny<Action<Exception>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(serviceSubscription.Object));
            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeQueryResponseAsync<BasicQueryMessage, BasicResponseMessage>(
                (msg) => Task.FromResult<QueryResponseMessage<BasicResponseMessage>>(null),
                (error) => { }
            );
            await subscription.EndAsync();
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithSynchronousActions()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var recievedActions = new List<Func<IRecievedServiceMessage, Task<IServiceMessage>>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<IRecievedServiceMessage>();

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                Capture.In<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(recievedActions),
                Capture.In<Action<Exception>>(errorActions),
                Capture.In<string>(channels),
                Capture.In<string>(groups),
                It.IsAny<IServiceChannelOptions>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IServiceSubscription>(serviceSubscription.Object));
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(async (IServiceMessage message, TimeSpan timeout, IServiceChannelOptions options, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceRecievedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    var result = await recievedActions[0](rmessage);
                    return Helper.ProduceQueryResult(result);
                });

            var contractConnection = new ContractConnection(serviceConnection.Object);

            var message1 = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects1");
            var message2 = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects2");
            var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeQueryResponseWithNoExtendedAspects");
            #endregion

            #region Act
            var messages = new List<IMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeQueryResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) => {
                messages.Add(msg);
                return Task.FromResult(new QueryResponseMessage<BasicResponseMessage>(responseMessage, null));
            }, (error) => exceptions.Add(error),
            synchronous:true);
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.QueryAsync<BasicQueryMessage>(message1);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();
            var result2 = await contractConnection.QueryAsync<BasicQueryMessage>(message2);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<IMessage<BasicQueryMessage>>(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result1);
            Assert.AreEqual(1, recievedActions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(2, serviceMessages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsFalse(string.IsNullOrWhiteSpace(groups[0]));
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(serviceMessages[0].RecievedTimestamp, messages[0].RecievedTimestamp);
            Assert.AreEqual(message1, messages[0].Message);
            Assert.AreEqual(message2, messages[1].Message);
            Assert.AreEqual(exception, exceptions[0]);
            Assert.IsFalse(result1.IsError);
            Assert.IsNull(result1.Error);
            Assert.AreEqual(result1.Result, responseMessage);
            Assert.IsFalse(result2.IsError);
            Assert.IsNull(result2.Error);
            Assert.AreEqual(result2.Result, responseMessage);
            System.Diagnostics.Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].RecievedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncErrorTriggeringInOurAction()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var recievedActions = new List<Func<IRecievedServiceMessage, Task<IServiceMessage>>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<IRecievedServiceMessage>();

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                Capture.In<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(recievedActions),
                Capture.In<Action<Exception>>(errorActions),
                Capture.In<string>(channels),
                Capture.In<string>(groups),
                It.IsAny<IServiceChannelOptions>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IServiceSubscription>(serviceSubscription.Object));
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(async (IServiceMessage message, TimeSpan timeout, IServiceChannelOptions options, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceRecievedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    var result = await recievedActions[0](rmessage);
                    return Helper.ProduceQueryResult(result);
                });

            var contractConnection = new ContractConnection(serviceConnection.Object);

            var message = new BasicQueryMessage("TestSubscribeQueryResponseAsyncErrorTriggeringInOurAction");
            var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseAsyncErrorTriggeringInOurAction");
            var exception = new NullReferenceException("TestSubscribeQueryResponseAsyncErrorTriggeringInOurAction");
            #endregion

            #region Act
            var messages = new List<IMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeQueryResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) => {
                throw exception;
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage>(message);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<Exception>(exceptions, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, recievedActions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(0, messages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsFalse(string.IsNullOrWhiteSpace(groups[0]));
            Assert.AreEqual(exception, exceptions[0]);
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(exception.Message, result.Error);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithDisposal()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var recievedActions = new List<Func<IRecievedServiceMessage, Task<IServiceMessage>>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<IRecievedServiceMessage>();

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                Capture.In<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(recievedActions),
                Capture.In<Action<Exception>>(errorActions),
                Capture.In<string>(channels),
                Capture.In<string>(groups),
                It.IsAny<IServiceChannelOptions>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IServiceSubscription>(serviceSubscription.Object));
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(async (IServiceMessage message, TimeSpan timeout, IServiceChannelOptions options, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceRecievedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    var result = await recievedActions[0](rmessage);
                    return Helper.ProduceQueryResult(result);
                });

            var contractConnection = new ContractConnection(serviceConnection.Object);

            var message = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeQueryResponseWithNoExtendedAspects");
            #endregion

            #region Act
            var messages = new List<IMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeQueryResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) => {
                messages.Add(msg);
                return Task.FromResult(new QueryResponseMessage<BasicResponseMessage>(responseMessage, null));
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage>(message);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<IMessage<BasicQueryMessage>>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, recievedActions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsFalse(string.IsNullOrWhiteSpace(groups[0]));
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(serviceMessages[0].RecievedTimestamp, messages[0].RecievedTimestamp);
            Assert.AreEqual(message, messages[0].Message);
            Assert.AreEqual(exception, exceptions[0]);
            Assert.IsFalse(result.IsError);
            Assert.IsNull(result.Error);
            Assert.AreEqual(result.Result, responseMessage);
            System.Diagnostics.Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].RecievedTimestamp).TotalMilliseconds}ms");
            Exception? disposeError = null;
            try
            {
                subscription.Dispose();
            }
            catch (Exception e)
            {
                disposeError=e;
            }
            Assert.IsNull(disposeError);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<IRecievedServiceMessage, Task<IServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

    }
}
