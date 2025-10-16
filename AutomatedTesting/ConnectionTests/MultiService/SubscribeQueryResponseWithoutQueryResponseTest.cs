using AutomatedTesting.Messages;
using Moq;
using MQContract;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Service;
using System.Diagnostics;
using System.Reflection;

namespace AutomatedTesting.ConnectionTests.MultiService
{
    [TestClass]
    public class SubscribeQueryResponseWithoutQueryResponseTest
    {
        private const string ServiceName = "testService";

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithNoExtendedAspects()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceSubObject = serviceSubscription.Object;
            var testError = new Exception("this is a test error");

            var channels = new List<string>();
            var groups = new List<string>();
            List<ServiceMessage> messages = [];
            List<Action<ReceivedServiceMessage>> messageActions = [];
            List<Action<Exception>> errorHandlers = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(messageActions), Capture.In<Action<Exception>>(errorHandlers),
                Capture.In(channels), Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubObject);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    messages.Add(message);
                    var idx = channels.IndexOf(message.Channel);
                    if (idx != -1)
                        messageActions[idx](new ReceivedServiceMessage(message.ID, message.MessageTypeID, message.Channel, message.Header, message.Data));
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);

            var message = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            #endregion

            #region Act
            var receivedMessages = new List<IReceivedMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                receivedMessages.Add(msg);
                return ValueTask.FromResult(new QueryResponseMessage<BasicResponseMessage>(responseMessage, null));
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage>(message);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            errorHandlers.ForEach(eh => eh(testError));

            await subscription.EndAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(receivedMessages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(2, groups.Count);
            Assert.AreEqual(2, messages.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.IsNotNull(groups[1]);
            Assert.AreEqual(receivedMessages[0].ID, messages[0].ID);
            Assert.AreEqual(0, receivedMessages[0].Headers.Keys.Count());
            Assert.AreEqual(3, messages[0].Header.Keys.Count());
            Assert.AreEqual(message, receivedMessages[0].Message);
            Assert.IsFalse(result.First().IsError);
            Assert.IsNull(result.First().Error);
            Assert.AreEqual(result.First().Result, responseMessage);
            Assert.AreEqual(2, errorHandlers.Count);
            Assert.AreEqual(testError, exceptions[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithInvalidHeadersOnResponseChannel()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceSubObject = serviceSubscription.Object;
            var testError = new Exception("this is a test error");

            var channels = new List<string>();
            var groups = new List<string>();
            List<ServiceMessage> messages = [];
            List<Action<ReceivedServiceMessage>> messageActions = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In(channels), Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubObject);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    messages.Add(message);
                    var idx = channels.IndexOf(message.Channel);
                    if (idx != -1)
                        messageActions[idx](new ReceivedServiceMessage(message.ID, message.MessageTypeID, message.Channel, new([]), message.Data));
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);

            var message = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            #endregion

            #region Act
            var receivedMessages = new List<IReceivedMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                receivedMessages.Add(msg);
                return ValueTask.FromResult(new QueryResponseMessage<BasicResponseMessage>(responseMessage, null));
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var error = await Assert.ThrowsExactlyAsync<QueryTimeoutException>(async () => _ = await contractConnection.QueryAsync<BasicQueryMessage>(message, timeout: TimeSpan.FromSeconds(2)));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            await subscription.EndAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(error);
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(2, groups.Count);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.IsNotNull(groups[1]);
            Assert.AreEqual(0, receivedMessages.Count);
            Assert.AreEqual(3, messages[0].Header.Keys.Count());
            Assert.IsInstanceOfType<InvalidQueryResponseMessageReceivedException>(exceptions[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task TestSubscribeQueryResponseAsyncWithTelemetryData(bool withLinking)
        {
            #region Arrange
            (var listener, var capturedActivities, var sourceName) = ConnectionHelper.SetupTelemetry();
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceSubObject = serviceSubscription.Object;
            var testError = new Exception("this is a test error");

            var channels = new List<string>();
            var groups = new List<string>();
            List<ServiceMessage> messages = [];
            List<Action<ReceivedServiceMessage>> messageActions = [];
            List<Action<Exception>> errorHandlers = [];
            List<ReceivedServiceMessage> receivedServiceMessages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(messageActions), Capture.In<Action<Exception>>(errorHandlers),
                Capture.In(channels), Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubObject);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    messages.Add(message);
                    var idx = channels.IndexOf(message.Channel);
                    var receivedMessage = new ReceivedServiceMessage(message.ID, message.MessageTypeID, message.Channel, message.Header, message.Data);
                    receivedServiceMessages.Add(receivedMessage);
                    if (idx != -1)
                        messageActions[idx](receivedMessage);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: withLinking);

            var message = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            #endregion

            #region Act
            var receivedMessages = new List<IReceivedMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                receivedMessages.Add(msg);
                return ValueTask.FromResult(new QueryResponseMessage<BasicResponseMessage>(responseMessage, null));
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage>(message);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            errorHandlers.ForEach(eh => eh(testError));

            await subscription.EndAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(receivedMessages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(2, groups.Count);
            Assert.AreEqual(2, messages.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.IsNotNull(groups[1]);
            Assert.AreEqual(receivedMessages[0].ID, messages[0].ID);
            Assert.AreEqual((withLinking ? 2 : 0), receivedMessages[0].Headers.Keys.Count());
            Assert.AreEqual((withLinking ? 5 : 3), messages[0].Header.Keys.Count());
            Assert.AreEqual(message, receivedMessages[0].Message);
            Assert.IsFalse(result.First().IsError);
            Assert.IsNull(result.First().Error);
            Assert.AreEqual(result.First().Result, responseMessage);
            Assert.AreEqual(2, errorHandlers.Count);
            Assert.AreEqual(testError, exceptions[0]);
            Assert.AreEqual(4, capturedActivities.Count);
            ConnectionHelper.ValidateConsumeActivity<BasicQueryMessage>(
                receivedServiceMessages[0],
                capturedActivities[1],
                "MQContract.ConsumeQueryMessage",
                serviceConnection.Object.GetType(),
                true,
                withLinking,
                connectionName: ServiceName
            );
            ConnectionHelper.ValidatePublishActivity<BasicResponseMessage>(
                messages[1],
                capturedActivities[2],
                "MQContract.ProduceQueryResponse",
                serviceConnection.Object.GetType(),
                true,
                withLinking,
                connectionName: ServiceName
            );
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
            #endregion
        }
    }
}
