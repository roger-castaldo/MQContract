using AutomatedTesting.Messages;
using Moq;
using MQContract;
using MQContract.Attributes;
using MQContract.Interfaces.Service;
using Polly.CircuitBreaker;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace AutomatedTesting.ConnectionTests.MappedService
{
    [TestClass]
    public class QueryWithoutQueryResponseTests
    {
        private const string ServiceName = "testService";
        private const string REPLY_CHANNEL_HEADER = "_QueryReplyChannel";
        private const string QUERY_IDENTIFIER_HEADER = "_QueryClientID";
        private const string REPLY_ID = "_QueryReplyID";

        [TestMethod]
        public async Task TestQueryAsyncWithNoExtendedAspects()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            var responseChannel = "BasicQuery.Response";
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();
            var acknowledged = false;


            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];
            List<Action<ReceivedServiceMessage>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In(channels), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    messages.Add(message);
                    var resp = new ReceivedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", responseChannel, message.Header, responseData, () =>
                    {
                        acknowledged=true;
                        return ValueTask.CompletedTask;
                    });
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(responseChannel, channels[0]);
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(messages[0].Data.Length > 0);
            Assert.AreEqual(3, messages[0].Header.Keys.Count());
            Assert.AreEqual(responseChannel, messages[0].Header[REPLY_CHANNEL_HEADER]);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            Assert.IsTrue(acknowledged);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithAHeaderValue()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            var responseChannel = "BasicQuery.Response";
            var headerKey = "MyHeaderKey";
            var headerValue = "MyHeaderValue";
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];
            List<Action<ReceivedServiceMessage>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In(channels), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    messages.Add(message);
                    var resp = new ReceivedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", responseChannel, message.Header, responseData);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, messageHeader: new([new KeyValuePair<string, string>(headerKey, headerValue)]), responseChannel: responseChannel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(responseChannel, channels[0]);
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(messages[0].Data.Length > 0);
            Assert.AreEqual(4, messages[0].Header.Keys.Count());
            Assert.AreEqual(responseChannel, messages[0].Header[REPLY_CHANNEL_HEADER]);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            Assert.AreEqual(1, result.Header.Keys.Count());
            Assert.AreEqual(headerValue, result.Header[headerKey]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithASecondMessageNotMatchingTheRequestBeingSent()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            var responseChannel = "BasicQuery.Response";
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];
            List<Action<ReceivedServiceMessage>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In(channels), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    messages.Add(message);
                    var resp = new ReceivedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", responseChannel, new([
                        new KeyValuePair<string,string>(QUERY_IDENTIFIER_HEADER,Guid.NewGuid().ToString()),
                        new KeyValuePair<string,string>(REPLY_ID,Guid.NewGuid().ToString()),
                        new KeyValuePair<string,string>(REPLY_CHANNEL_HEADER,responseChannel)
                        ]), responseData);
                    foreach (var action in messageActions)
                        action(resp);
                    resp = new ReceivedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", responseChannel, message.Header, responseData);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(responseChannel, channels[0]);
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(messages[0].Data.Length > 0);
            Assert.AreEqual(3, messages[0].Header.Keys.Count());
            Assert.AreEqual(responseChannel, messages[0].Header[REPLY_CHANNEL_HEADER]);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithTheAttributeChannel()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            var responseChannel = typeof(BasicQueryMessage).GetCustomAttribute<QueryResponseChannelAttribute>()?.Name;
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];
            List<Action<ReceivedServiceMessage>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In(channels), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    messages.Add(message);
                    var resp = new ReceivedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", responseChannel!, message.Header, responseData);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(responseChannel, channels[0]);
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(messages[0].Data.Length > 0);
            Assert.AreEqual(3, messages[0].Header.Keys.Count());
            Assert.AreEqual(responseChannel, messages[0].Header[REPLY_CHANNEL_HEADER]);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncFailingToCreateSubscription()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseChannel = "BasicQuery.Response";

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult<IServiceSubscription?>(null));

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var error = await Assert.ThrowsExceptionAsync<QueryExecutionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncFailingWithNoResponseChannel()
        {
            #region Arrange
            var testMessage = new BasicResponseMessage("testMessage");

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult<IServiceSubscription?>(null));

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var error = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await contractConnection.QueryAsync<BasicResponseMessage, BasicResponseMessage>(testMessage, channel: "Test"));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            Assert.AreEqual("responseChannel", error.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithTimeoutException()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseChannel = "BasicQuery.Response";
            var defaultTimeout = TimeSpan.FromSeconds(5);

            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];
            List<Action<ReceivedServiceMessage>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In(channels), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    messages.Add(message);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var error = await Assert.ThrowsExceptionAsync<QueryTimeoutException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel, timeout: defaultTimeout));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task TestQueryAsyncWithTelemetryData(bool withLinking)
        {
            #region Arrange
            (var listener, var capturedActivities, var sourceName) = ConnectionHelper.SetupTelemetry();
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            var responseChannel = "BasicQuery.Response";
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();
            var acknowledged = false;


            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];
            List<Action<ReceivedServiceMessage>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In(channels), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    messages.Add(message);
                    var resp = new ReceivedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", responseChannel, message.Header, responseData, () =>
                    {
                        acknowledged=true;
                        return ValueTask.CompletedTask;
                    });
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = ContractConnection.MappedServiceInstance()
                .RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object)
                .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: withLinking);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(responseChannel, channels[0]);
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(messages[0].Data.Length > 0);
            Assert.AreEqual((withLinking ? 5 : 3), messages[0].Header.Keys.Count());
            Assert.AreEqual(responseChannel, messages[0].Header[REPLY_CHANNEL_HEADER]);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            Assert.IsTrue(acknowledged);
            Assert.AreEqual(2, capturedActivities.Count);
            ConnectionHelper.ValidatePublishActivity<BasicQueryMessage>(
                 messages[0],
                 capturedActivities[0],
                 "MQContract.PublishQueryMessage",
                 serviceConnection.Object.GetType(),
                 true,
                 withLinking,
                 connectionName: ServiceName
             );
            ConnectionHelper.ValidateConsumeActivity<BasicResponseMessage>(
                new ReceivedServiceMessage(messages[0].ID, "U-BasicResponseMessage-0.0.0.0", responseChannel, messages[0].Header, responseData),
                capturedActivities[1],
                "MQContract.ConsumeQueryResponse",
                serviceConnection.Object.GetType(),
                true,
                withLinking,
                connectionName: ServiceName
            );
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicQueryMessage), false)]
        public async Task TestQueryAsyncAndRetryFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseChannel = "BasicQuery.Response";
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString(), Error: new(new Exception("error occured"), false));
            var retryCount = 2;


            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(transmissionResult));

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, null);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var exception = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, responseChannel: responseChannel));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, retryCount+1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(exception);
            Assert.IsNotNull(exception.InnerException);
            Assert.IsInstanceOfType<ResilienceException>(exception.InnerException);
            Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)exception.InnerException).Type);
            Assert.IsNotNull(exception.InnerException.InnerException);
            Assert.AreEqual(transmissionResult.Error!.Exception, exception.InnerException.InnerException);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicQueryMessage), false)]
        public async Task TestQueryAsyncAndCircuitBreakFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseChannel = "BasicQuery.Response";
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString(), Error: new(new Exception("error occured"), false));
            var circuitBreakCount = 1;


            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(transmissionResult));

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, null, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            _ = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel));
            var exception = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, responseChannel: responseChannel));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, circuitBreakCount, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(exception);
            Assert.IsNotNull(exception.InnerException);
            Assert.IsInstanceOfType<ResilienceException>(exception.InnerException);
            Assert.AreEqual(ResilienceTypes.CircuitBreak, ((ResilienceException)exception.InnerException).Type);
            Assert.IsNotNull(exception.InnerException.InnerException);
            Assert.IsInstanceOfType<BrokenCircuitException>(exception.InnerException.InnerException);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicQueryMessage), false)]
        public async Task TestQueryAsyncWithRetryAndCircuitBreakWithFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseChannel = "BasicQuery.Response";
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString(), Error: new(new Exception("error occured"), false));
            var circuitBreakCount = 2;
            var retryCount = 1;


            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(transmissionResult));

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var retryException = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, responseChannel: responseChannel));
            var circuitException = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, responseChannel: responseChannel));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, retryCount+1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(retryException);
            Assert.IsNotNull(retryException.InnerException);
            Assert.IsInstanceOfType<ResilienceException>(retryException.InnerException);
            Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)retryException.InnerException).Type);
            Assert.IsNotNull(retryException.InnerException.InnerException);
            Assert.AreEqual(transmissionResult.Error!.Exception, retryException.InnerException.InnerException);
            Assert.IsNotNull(circuitException);
            Assert.IsNotNull(circuitException.InnerException);
            Assert.IsInstanceOfType<ResilienceException>(circuitException.InnerException);
            Assert.AreEqual(ResilienceTypes.CircuitBreak, ((ResilienceException)circuitException.InnerException).Type);
            Assert.IsNotNull(circuitException.InnerException.InnerException);
            Assert.IsInstanceOfType<BrokenCircuitException>(circuitException.InnerException.InnerException);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicQueryMessage), false)]
        public async Task TestQueryAsyncWithRetryAndCircuitBreakWithoutTripping(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseChannel = "BasicQuery.Response";
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString(), Error: new(new Exception("error occured"), true));
            var circuitBreakCount = 2;
            var retryCount = 1;


            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(transmissionResult));

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var retryException = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, responseChannel: responseChannel));
            var circuitException = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, responseChannel: responseChannel));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(retryException);
            Assert.IsNotNull(retryException.InnerException);
            Assert.IsNotInstanceOfType<ResilienceException>(retryException.InnerException);
            Assert.AreEqual(transmissionResult.Error!.Exception, retryException.InnerException);
            Assert.IsNotNull(circuitException);
            Assert.IsNotNull(circuitException.InnerException);
            Assert.IsNotInstanceOfType<ResilienceException>(circuitException.InnerException);
            Assert.AreEqual(transmissionResult.Error!.Exception, circuitException.InnerException);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithRetryAndCircuitBreakFailureAndEnsuringPriority()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseChannel = "BasicQuery.Response";
            var error = new Exception("test error");
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, false));
            var channel = "testChannel";
            var messageType = typeof(BasicQueryMessage);
            var circuitBreakCount = 2;
            var retryCount = 1;


            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(transmissionResult));

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, null, false, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, null, messageType, false, retryCount+1, circuitBreakCount+1);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var channelRetryException = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, responseChannel: responseChannel));
            var channelCircuitException = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, responseChannel: responseChannel));
            var typeRetryException = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel));
            var typeCircuitException = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, retryCount+1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(channelRetryException);
            Assert.IsNotNull(channelCircuitException);
            Assert.IsNotNull(typeRetryException);
            Assert.IsNotNull(typeCircuitException);

            Assert.IsTrue(Array.TrueForAll([channelRetryException, typeRetryException], (ex) => ex.InnerException is ResilienceException re
            && Equals(ResilienceTypes.Retry, re.Type)
            && Equals(error, re.InnerException)));

            Assert.IsTrue(Array.TrueForAll([channelCircuitException, typeCircuitException], (ex) => ex.InnerException is ResilienceException re
            && Equals(ResilienceTypes.CircuitBreak, re.Type)
            && re.InnerException is BrokenCircuitException));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(((retryCount+1)*2)+1));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
            mockSubscription.Verify(x => x.EndAsync(), Times.Exactly(4));
            #endregion
        }
    }
}
