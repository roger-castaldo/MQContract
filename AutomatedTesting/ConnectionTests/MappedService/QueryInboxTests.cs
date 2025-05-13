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
            Assert.AreEqual("U-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
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
            var exception = await Assert.ThrowsExceptionAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            Assert.IsNotNull(exception.InnerException);
            Assert.AreEqual(errorMessage, exception.InnerException.Message);
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
            var exception = await Assert.ThrowsExceptionAsync<QueryTimeoutException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage));
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
        public async Task TestQueryAsyncWithDisposableSubscriptionFromDispose()
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
            contractConnection.Dispose();
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
            Assert.AreEqual("U-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
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
        public async Task TestQueryAsyncWithAsyncDisposableSubscriptionFromDispose()
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
            contractConnection.Dispose();
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
            Assert.AreEqual("U-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
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
            Assert.AreEqual("U-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
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
            Assert.AreEqual("U-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
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
            Assert.AreEqual("U-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
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
            var exception = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, retryCount+1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(exception);
            Assert.IsNotNull(exception.InnerException);
            Assert.IsInstanceOfType<ResilienceException>(exception.InnerException);
            Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)exception.InnerException).Type);
            Assert.IsNotNull(exception.InnerException.InnerException);
            Assert.AreEqual(error, exception.InnerException.InnerException);
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
            _ = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel));
            var exception = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
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
            var retryException = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel));
            var circuitException = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, retryCount+1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(retryException);
            Assert.IsNotNull(retryException.InnerException);
            Assert.IsInstanceOfType<ResilienceException>(retryException.InnerException);
            Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)retryException.InnerException).Type);
            Assert.IsNotNull(retryException.InnerException.InnerException);
            Assert.AreEqual(error, retryException.InnerException.InnerException);
            Assert.IsNotNull(circuitException);
            Assert.IsNotNull(circuitException.InnerException);
            Assert.IsInstanceOfType<ResilienceException>(circuitException.InnerException);
            Assert.AreEqual(ResilienceTypes.CircuitBreak, ((ResilienceException)circuitException.InnerException).Type);
            Assert.IsNotNull(circuitException.InnerException.InnerException);
            Assert.IsInstanceOfType<BrokenCircuitException>(circuitException.InnerException.InnerException);
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
            var retryException = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel));
            var circuitException = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(retryException);
            Assert.IsNotNull(retryException.InnerException);
            Assert.IsNotInstanceOfType<ResilienceException>(retryException.InnerException);
            Assert.AreEqual(error, retryException.InnerException);
            Assert.IsNotNull(circuitException);
            Assert.IsNotNull(circuitException.InnerException);
            Assert.IsNotInstanceOfType<ResilienceException>(circuitException.InnerException);
            Assert.AreEqual(error, circuitException.InnerException);
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
            var channelRetryException = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel));
            var channelCircuitException = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel));
            var typeRetryException = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage));
            var typeCircuitException = await Assert.ThrowsAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
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
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(((retryCount+1)*2)+1));
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }
    }
}
