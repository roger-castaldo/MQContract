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
    public class QueryInboxTests
    {
        private const string ServiceName = "testService";
        [TestMethod]
        public async Task TestQueryAsyncWithNoExtendedAspects()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);

            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<Action<ReceivedInboxServiceMessage>> receivedActions = [];
            List<ServiceMessage> messages = [];
            List<Guid> messageIDs = [];
            var acknowledgeCount = 0;


            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(Capture.In(receivedActions), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(messageIDs), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, Guid messageID, CancellationToken cancellationToken) =>
                {
                    foreach (var action in receivedActions)
                        action(new(
                            queryResult.ID,
                            queryResult.MessageTypeID,
                            message.Channel,
                            queryResult.Header,
                            messageID,
                            queryResult.Data,
                            () =>
                            {
                                acknowledgeCount++;
                                return ValueTask.CompletedTask;
                            }
                        ));
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, messageIDs.Count);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length > 0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithErrorInPublish()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var errorMessage = "Unable to transmit";

            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromMinutes(1);

            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, Guid messageID, CancellationToken cancellationToken) =>
                {
                    return ValueTask.FromResult(new TransmissionResult(message.ID, Error: new(new Exception(errorMessage), true)));
                });
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsError);
            Assert.IsNotNull(result.Error);
            Assert.AreEqual(errorMessage, result.Error.Message);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithTimeout()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");

            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromSeconds(5);

            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, Guid messageID, CancellationToken cancellationToken) =>
                {
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var exception = await Assert.ThrowsExactlyAsync<QueryTimeoutException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithDisposableSubscriptionFromAsyncDispose()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);

            var mockSubscription = new Mock<IDisposable>().As<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<Action<ReceivedInboxServiceMessage>> receivedActions = [];
            List<ServiceMessage> messages = [];
            List<Guid> messageIDs = [];
            var acknowledgeCount = 0;


            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(Capture.In(receivedActions), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(messageIDs), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, Guid messageID, CancellationToken cancellationToken) =>
                {
                    foreach (var action in receivedActions)
                        action(new(
                            queryResult.ID,
                            queryResult.MessageTypeID,
                            message.Channel,
                            queryResult.Header,
                            messageID,
                            queryResult.Data,
                            () =>
                            {
                                acknowledgeCount++;
                                return ValueTask.CompletedTask;
                            }
                        ));
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            await contractConnection.DisposeAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, messageIDs.Count);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length > 0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            mockSubscription.As<IDisposable>().Verify(x => x.Dispose(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithAsyncDisposableSubscriptionFromDisposeAsync()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);

            var mockSubscription = new Mock<IServiceSubscription>();
            mockSubscription.As<IAsyncDisposable>()
                .Setup(x => x.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<Action<ReceivedInboxServiceMessage>> receivedActions = [];
            List<ServiceMessage> messages = [];
            List<Guid> messageIDs = [];
            var acknowledgeCount = 0;


            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(Capture.In(receivedActions), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(messageIDs), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, Guid messageID, CancellationToken cancellationToken) =>
                {
                    foreach (var action in receivedActions)
                        action(new(
                            queryResult.ID,
                            queryResult.MessageTypeID,
                            message.Channel,
                            queryResult.Header,
                            messageID,
                            queryResult.Data,
                            () =>
                            {
                                acknowledgeCount++;
                                return ValueTask.CompletedTask;
                            }
                        ));
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            await contractConnection.DisposeAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, messageIDs.Count);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length > 0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            mockSubscription.As<IAsyncDisposable>().Verify(x => x.DisposeAsync(), Times.Once);
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
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);

            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<Action<ReceivedInboxServiceMessage>> receivedActions = [];
            List<ServiceMessage> messages = [];
            List<Guid> messageIDs = [];
            var acknowledgeCount = 0;


            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(Capture.In(receivedActions), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(messageIDs), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, Guid messageID, CancellationToken cancellationToken) =>
                {
                    foreach (var action in receivedActions)
                        action(new(
                            queryResult.ID,
                            queryResult.MessageTypeID,
                            message.Channel,
                            queryResult.Header,
                            messageID,
                            queryResult.Data,
                            () =>
                            {
                                acknowledgeCount++;
                                return ValueTask.CompletedTask;
                            }
                        ));
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MappedServiceInstance()
                .RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object)
                .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: withLinking);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, messageIDs.Count);
            Assert.AreEqual((withLinking ? 2 : 0), messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length > 0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
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
                queryResult,
                capturedActivities[1],
                "MQContract.ConsumeQueryResponse",
                serviceConnection.Object.GetType(),
                true,
                connectionName: ServiceName
            );
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
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

            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, false))));
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, null);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, retryCount+1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsError);
            Assert.IsNotNull(result.Error);
            Assert.IsInstanceOfType<ResilienceException>(result.Error.Exception);
            Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)result.Error.Exception).Type);
            Assert.IsNotNull(result.Error.Exception.InnerException);
            Assert.AreEqual(error, result.Error.Exception.InnerException);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
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
            var error = new Exception("test error");
            var circuitBreakCount = 1;

            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, false))));
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, null, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            _ = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, circuitBreakCount, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsError);
            Assert.IsNotNull(result.Error);
            Assert.IsInstanceOfType<ResilienceException>(result.Error.Exception);
            Assert.AreEqual(ResilienceTypes.CircuitBreak, ((ResilienceException)result.Error.Exception).Type);
            Assert.IsNotNull(result.Error.Exception.InnerException);
            Assert.IsInstanceOfType<BrokenCircuitException>(result.Error.Exception.InnerException);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
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
            var error = new Exception("test error");
            var circuitBreakCount = 2;
            var retryCount = 1;

            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, false))));
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var retryResult = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            var circuitResult = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, retryCount+1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(retryResult);
            Assert.IsTrue(retryResult.IsError);
            Assert.IsNotNull(retryResult.Error);
            Assert.IsInstanceOfType<ResilienceException>(retryResult.Error.Exception);
            Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)retryResult.Error.Exception).Type);
            Assert.IsNotNull(retryResult.Error.Exception.InnerException);
            Assert.AreEqual(error, retryResult.Error.Exception.InnerException);
            Assert.IsNotNull(circuitResult);
            Assert.IsTrue(circuitResult.IsError);
            Assert.IsNotNull(circuitResult.Error);
            Assert.IsInstanceOfType<ResilienceException>(circuitResult.Error.Exception);
            Assert.AreEqual(ResilienceTypes.CircuitBreak, ((ResilienceException)circuitResult.Error.Exception).Type);
            Assert.IsNotNull(circuitResult.Error.Exception.InnerException);
            Assert.IsInstanceOfType<BrokenCircuitException>(circuitResult.Error.Exception.InnerException);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
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
            var error = new Exception("test error");
            var circuitBreakCount = 2;
            var retryCount = 1;

            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, true))));
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var retryResult = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            var circuitResult = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(retryResult);
            Assert.IsTrue(retryResult.IsError);
            Assert.IsNotNull(retryResult.Error);
            Assert.IsNotInstanceOfType<ResilienceException>(retryResult.Error.Exception);
            Assert.AreEqual(error, retryResult.Error.Exception);
            Assert.IsNotNull(circuitResult);
            Assert.IsTrue(circuitResult.IsError);
            Assert.IsNotNull(circuitResult.Error);
            Assert.IsNotInstanceOfType<ResilienceException>(circuitResult.Error.Exception);
            Assert.AreEqual(error, circuitResult.Error.Exception);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithRetryAndCircuitBreakFailureAndEnsuringPriority()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var error = new Exception("test error");
            var channel = "testChannel";
            var messageType = typeof(BasicQueryMessage);
            var circuitBreakCount = 2;
            var retryCount = 1;

            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, false))));
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, null, false, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, null, messageType, false, retryCount+1, circuitBreakCount+1);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var channelRetryResult = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            var channelCircuitResult = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            var typeRetryResult = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            var typeCircuitResult = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, retryCount+1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(channelRetryResult);
            Assert.IsNotNull(channelCircuitResult);
            Assert.IsNotNull(typeRetryResult);
            Assert.IsNotNull(typeCircuitResult);

            Assert.IsTrue(Array.TrueForAll([channelRetryResult, typeRetryResult], (result) => result.IsError
            && result.Error!=null
            && result.Error.Exception is ResilienceException re
            && Equals(ResilienceTypes.Retry, re.Type)
            && Equals(error, re.InnerException)));

            Assert.IsTrue(Array.TrueForAll([channelCircuitResult, typeCircuitResult], (result) => result.IsError
            && result.Error!=null
            && result.Error.Exception is ResilienceException re
            && Equals(ResilienceTypes.CircuitBreak, re.Type)
            && re.InnerException is BrokenCircuitException));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(((retryCount+1)*2)+1));
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }
    }
}
