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
    public class QueryInboxTests
    {
        private const string ServiceName = "testService";
        private const string ServiceName2 = "testService2";

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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
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
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, messageIDs);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.First().Result);
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
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
            Assert.IsTrue(Array.TrueForAll(result.ToArray(), r => r.IsError
            && r.Error!=null
            && Equals(errorMessage, r.Error.Exception.Message)));
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
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
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, messageIDs);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.First().Result);
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
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
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, messageIDs);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.First().Result);
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
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
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, messageIDs);
            Assert.AreEqual((withLinking ? 2 : 0), messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.First().Result);
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
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);

            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromMinutes(1);
            List<Action<ReceivedInboxServiceMessage>> receivedActions = [];
            var acknowledgeCount = 0;

            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, false))));
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);
            var serviceConnection2 = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection2.Setup(x => x.EstablishInboxSubscriptionAsync(Capture.In(receivedActions), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection2.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
            serviceConnection2.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, null);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, retryCount, null);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var results = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
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
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection2.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection2.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
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
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);

            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromMinutes(1);
            List<Action<ReceivedInboxServiceMessage>> receivedActions = [];
            var acknowledgeCount = 0;

            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, false))));
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);
            var serviceConnection2 = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection2.Setup(x => x.EstablishInboxSubscriptionAsync(Capture.In(receivedActions), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection2.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
            serviceConnection2.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, null, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, null, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            _ = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            var results = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
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
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection2.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection2.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
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
            var error = new Exception("test error");
            var circuitBreakCount = 2;
            var retryCount = 1;
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);

            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromMinutes(1);
            List<Action<ReceivedInboxServiceMessage>> receivedActions = [];
            var acknowledgeCount = 0;

            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, false))));
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);
            var serviceConnection2 = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection2.Setup(x => x.EstablishInboxSubscriptionAsync(Capture.In(receivedActions), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection2.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
            serviceConnection2.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var retryResults = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            var circuitResults = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
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
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection2.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection2.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
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
            var error = new Exception("test error");
            var circuitBreakCount = 2;
            var retryCount = 1;
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);

            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromMinutes(1);
            List<Action<ReceivedInboxServiceMessage>> receivedActions = [];
            var acknowledgeCount = 0;

            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, true))));
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);
            var serviceConnection2 = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection2.Setup(x => x.EstablishInboxSubscriptionAsync(Capture.In(receivedActions), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection2.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
            serviceConnection2.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicQueryMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var retryResults = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            var circuitResults = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
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
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection2.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection2.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
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
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);

            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromMinutes(1);
            List<Action<ReceivedInboxServiceMessage>> receivedActions = [];
            var acknowledgeCount = 0;

            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, false))));
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);
            var serviceConnection2 = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection2.Setup(x => x.EstablishInboxSubscriptionAsync(Capture.In(receivedActions), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection2.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
            serviceConnection2.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);
            var serviceConnection3 = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection3.Setup(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection3.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, false))));
            serviceConnection3.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

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
            var retryResults = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            var circuitResults = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
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
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection2.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection2.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection3.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection3.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Exactly(3));
            #endregion
        }
    }
}
