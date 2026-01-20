using AutomatedTesting.Messages;
using Moq;
using MQContract;
using MQContract.Attributes;
using MQContract.Interfaces.Service;
using Polly.CircuitBreaker;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace AutomatedTesting.ConnectionTests.MultiService
{
    [TestClass]
    public class QueryWithoutQueryResponseTests
    {
        private const string ServiceName = "testService";
        private const string ServiceName2 = "testService2";
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
            await JsonSerializer.SerializeAsync(ms, responseMessage, cancellationToken: TestContext.CancellationToken);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();
            var acknowledged = false;


            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];
            List<Func<ReceivedServiceMessage, ValueTask>> messageActions = [];
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, channels);
            Assert.AreEqual(responseChannel, channels[0]);
            Assert.HasCount(1, messages);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(3, messages[0].Header.Keys.Count());
            Assert.AreEqual(responseChannel, messages[0].Header[REPLY_CHANNEL_HEADER]);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: TestContext.CancellationToken));
            Assert.AreEqual(responseMessage, result.First().Result);
            Assert.IsTrue(acknowledged);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
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
            await JsonSerializer.SerializeAsync(ms, responseMessage, cancellationToken: TestContext.CancellationToken);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];
            List<Func<ReceivedServiceMessage, ValueTask>> messageActions = [];
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, messageHeader: new([new KeyValuePair<string, string>(headerKey, headerValue)]), responseChannel: responseChannel, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, channels);
            Assert.AreEqual(responseChannel, channels[0]);
            Assert.HasCount(1, messages);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(4, messages[0].Header.Keys.Count());
            Assert.AreEqual(responseChannel, messages[0].Header[REPLY_CHANNEL_HEADER]);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: TestContext.CancellationToken));
            Assert.AreEqual(responseMessage, result.First().Result);
            Assert.AreEqual(1, result.First().Header.Keys.Count());
            Assert.AreEqual(headerValue, result.First().Header[headerKey]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
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
            await JsonSerializer.SerializeAsync(ms, responseMessage, cancellationToken: TestContext.CancellationToken);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];
            List<Func<ReceivedServiceMessage, ValueTask>> messageActions = [];
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, channels);
            Assert.AreEqual(responseChannel, channels[0]);
            Assert.HasCount(1, messages);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(3, messages[0].Header.Keys.Count());
            Assert.AreEqual(responseChannel, messages[0].Header[REPLY_CHANNEL_HEADER]);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: TestContext.CancellationToken));
            Assert.AreEqual(responseMessage, result.First().Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
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
            var responseChannel = typeof(BasicQueryMessage).GetCustomAttribute<QueryMessageAttribute>()?.ResponseChannel;
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage, cancellationToken: TestContext.CancellationToken);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];
            List<Func<ReceivedServiceMessage, ValueTask>> messageActions = [];
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, channels);
            Assert.AreEqual(responseChannel, channels[0]);
            Assert.HasCount(1, messages);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(3, messages[0].Header.Keys.Count());
            Assert.AreEqual(responseChannel, messages[0].Header[REPLY_CHANNEL_HEADER]);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: TestContext.CancellationToken));
            Assert.AreEqual(responseMessage, result.First().Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
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
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult<IServiceSubscription?>(null));

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var error = await Assert.ThrowsExactlyAsync<QueryExecutionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel, cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncFailingWithNoResponseChannel()
        {
            #region Arrange
            var testMessage = new BasicResponseMessage("testMessage");

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult<IServiceSubscription?>(null));

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var error = await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await contractConnection.QueryAsync<BasicResponseMessage, BasicResponseMessage>(testMessage, channel: "Test", cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            Assert.AreEqual("responseChannel", error.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
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
            List<Func<ReceivedServiceMessage, ValueTask>> messageActions = [];
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var error = await Assert.ThrowsExactlyAsync<QueryTimeoutException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel, timeout: defaultTimeout, cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
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
            await JsonSerializer.SerializeAsync(ms, responseMessage, cancellationToken: TestContext.CancellationToken);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();
            var acknowledged = false;


            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];
            List<Func<ReceivedServiceMessage, ValueTask>> messageActions = [];
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: withLinking);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, channels);
            Assert.AreEqual(responseChannel, channels[0]);
            Assert.HasCount(1, messages);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual((withLinking ? 5 : 3), messages[0].Header.Keys.Count());
            Assert.AreEqual(responseChannel, messages[0].Header[REPLY_CHANNEL_HEADER]);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: TestContext.CancellationToken));
            Assert.AreEqual(responseMessage, result.First().Result);
            Assert.IsTrue(acknowledged);
            Assert.HasCount(2, capturedActivities);
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
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
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
            var error = new Exception("test error");
            var retryCount = 2;
            var transmitionError = new TransmissionException(error, false);
            var responseMessage = new BasicResponseMessage("testResponse");
            var responseChannel = "BasicQuery.Response";
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage, cancellationToken: TestContext.CancellationToken);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var mockSubscription = new Mock<IServiceSubscription>();

            List<Func<ReceivedServiceMessage, ValueTask>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(transmitionError, false)));
            var serviceConnection2 = new Mock<IMessageServiceConnection>();
            serviceConnection2.Setup(x => x.SubscribeAsync(Capture.In(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In(channels), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection2.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var resp = new ReceivedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", responseChannel, message.Header, responseData, null);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, null);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, retryCount, null);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var results = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any(result => result.IsError
            && result.Error!=null
            && result.Error.Exception is ResilienceException re
            && Equals(ResilienceTypes.Retry, re.Type)
            && re.InnerException != null
            && Equals(error, re.InnerException)));
            Assert.IsTrue(results.Any(result => !result.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection2.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection2.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
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
            var error = new Exception("test error");
            var circuitBreakCount = 1;
            var transmitionError = new TransmissionException(error, false);
            var responseMessage = new BasicResponseMessage("testResponse");
            var responseChannel = "BasicQuery.Response";
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage, cancellationToken: TestContext.CancellationToken);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var mockSubscription = new Mock<IServiceSubscription>();

            List<Func<ReceivedServiceMessage, ValueTask>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(transmitionError, false)));
            var serviceConnection2 = new Mock<IMessageServiceConnection>();
            serviceConnection2.Setup(x => x.SubscribeAsync(Capture.In(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In(channels), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection2.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var resp = new ReceivedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", responseChannel, message.Header, responseData, null);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, null, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, null, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            _ = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken);
            var results = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any(result => result.IsError
            && result.Error!=null
            && result.Error.Exception is ResilienceException re
            && Equals(ResilienceTypes.CircuitBreak, re.Type)
            && re.InnerException is BrokenCircuitException));
            Assert.IsTrue(results.Any(result => !result.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(circuitBreakCount));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection2.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection2.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockSubscription.Verify(x => x.EndAsync(), Times.Exactly(4));
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicQueryMessage), false)]
        public async Task TestQueryAsyncAndCircuitBreakWithFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var error = new Exception("test error");
            var circuitBreakCount = 2;
            var retryCount = 1;
            var transmitionError = new TransmissionException(error, false);
            var responseMessage = new BasicResponseMessage("testResponse");
            var responseChannel = "BasicQuery.Response";
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage, cancellationToken: TestContext.CancellationToken);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var mockSubscription = new Mock<IServiceSubscription>();

            List<Func<ReceivedServiceMessage, ValueTask>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(transmitionError, false)));
            var serviceConnection2 = new Mock<IMessageServiceConnection>();
            serviceConnection2.Setup(x => x.SubscribeAsync(Capture.In(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In(channels), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection2.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var resp = new ReceivedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", responseChannel, message.Header, responseData, null);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var retryResults = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken);
            var circuitResults = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(retryResults);
            Assert.IsTrue(retryResults.Any(result => result.IsError
            && result.Error!=null
            && result.Error.Exception is ResilienceException re
            && Equals(ResilienceTypes.Retry, re.Type)
            && re.InnerException != null
            && Equals(error, re.InnerException)));
            Assert.IsTrue(retryResults.Any(result => !result.IsError));

            Assert.IsNotNull(circuitResults);
            Assert.IsTrue(circuitResults.Any(result => result.IsError
            && result.Error!=null
            && result.Error.Exception is ResilienceException re
            && Equals(ResilienceTypes.CircuitBreak, re.Type)
            && re.InnerException is BrokenCircuitException));
            Assert.IsTrue(circuitResults.Any(result => !result.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection2.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection2.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockSubscription.Verify(x => x.EndAsync(), Times.Exactly(4));
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicQueryMessage), false)]
        public async Task TestQueryAsyncAndCircuitBreakWithoutTripping(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var error = new Exception("test error");
            var circuitBreakCount = 2;
            var retryCount = 1;
            var transmitionError = new TransmissionException(error, true);
            var responseMessage = new BasicResponseMessage("testResponse");
            var responseChannel = "BasicQuery.Response";
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage, cancellationToken: TestContext.CancellationToken);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var mockSubscription = new Mock<IServiceSubscription>();

            List<Func<ReceivedServiceMessage, ValueTask>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(transmitionError, false)));
            var serviceConnection2 = new Mock<IMessageServiceConnection>();
            serviceConnection2.Setup(x => x.SubscribeAsync(Capture.In(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In(channels), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection2.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var resp = new ReceivedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", responseChannel, message.Header, responseData, null);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var retryResults = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken);
            var circuitResults = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(retryResults);
            Assert.IsTrue(retryResults.Any(result => result.IsError
            && result.Error!=null
            && result.Error.Exception is not ResilienceException
            && Equals(error, result.Error.Exception)
            ));
            Assert.IsTrue(retryResults.Any(result => !result.IsError));

            Assert.IsNotNull(circuitResults);
            Assert.IsTrue(circuitResults.Any(result => result.IsError
            && result.Error!=null
            && result.Error.Exception is not ResilienceException
            && Equals(error, result.Error.Exception)
            ));
            Assert.IsTrue(circuitResults.Any(result => !result.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection2.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection2.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockSubscription.Verify(x => x.EndAsync(), Times.Exactly(4));
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithRetryAndCircuitBreakFailureAndEnsuringPriority()
        {
            #region Arrange
            var serviceName3 = "testService3";
            var testMessage = new BasicQueryMessage("testMessage");
            var error = new Exception("test error");
            var channel = "testChannel";
            var messageType = typeof(BasicQueryMessage);
            var circuitBreakCount = 2;
            var retryCount = 1;
            var transmitionError = new TransmissionException(error, false);
            var responseMessage = new BasicResponseMessage("testResponse");
            var responseChannel = "BasicQuery.Response";
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage, cancellationToken: TestContext.CancellationToken);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var mockSubscription = new Mock<IServiceSubscription>();

            List<Func<ReceivedServiceMessage, ValueTask>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(transmitionError, false)));
            var serviceConnection2 = new Mock<IMessageServiceConnection>();
            serviceConnection2.Setup(x => x.SubscribeAsync(Capture.In(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In(channels), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection2.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var resp = new ReceivedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", responseChannel, message.Header, responseData, null);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });
            var serviceConnection3 = new Mock<IMessageServiceConnection>();
            serviceConnection3.Setup(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection3.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(transmitionError, false)));

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .RegisterServiceConnection(ServiceName2, serviceConnection2.Object)
                .RegisterServiceConnection(serviceName3, serviceConnection3.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, null, false, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, null, messageType, false, retryCount+1, circuitBreakCount+1);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName2, channel, null, false, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName2, null, messageType, false, retryCount+1, circuitBreakCount+1);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, null, channel, null, false, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, null, null, messageType, false, retryCount+1, circuitBreakCount+1);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var retryResults = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken);
            var circuitResults = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(retryResults);
            Assert.AreEqual(2, retryResults.Count(result => result.IsError
            && result.Error!=null
            && result.Error.Exception is ResilienceException re
            && Equals(ResilienceTypes.Retry, re.Type)
            && re.InnerException != null
            && Equals(error, re.InnerException)));
            Assert.AreEqual(1, retryResults.Count(result => !result.IsError));

            Assert.IsNotNull(circuitResults);
            Assert.AreEqual(2, circuitResults.Count(result => result.IsError
            && result.Error!=null
            && result.Error.Exception is ResilienceException re
            && Equals(ResilienceTypes.CircuitBreak, re.Type)
            && re.InnerException is BrokenCircuitException));
            Assert.AreEqual(1, circuitResults.Count(result => !result.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection2.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection2.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection3.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection3.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockSubscription.Verify(x => x.EndAsync(), Times.Exactly(6));
            #endregion
        }

        public TestContext TestContext { get; set; }
    }
}
