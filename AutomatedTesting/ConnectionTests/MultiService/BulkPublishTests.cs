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
    public class BulkPublishTests
    {
        private const string ServiceName = "testService";
        private const string ServiceName2 = "testService2";

        [TestMethod]
        public async Task TestBulkPublishAsyncWithNoBulkSupportAndBasicCall()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            IEnumerable<(BasicMessage message, MessageHeader? messageHeader)> testMessages = [
                (new("testMessage"),null),
                (new("testMessage2"),null)
            ];

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MultiServiceInstance();
            contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, testMessages.Count(), TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsTrue(result.All(r => r.Results.Count()==1));
            Assert.IsTrue(result.SelectMany(r => r.Results).All(r => !r.IsError && Equals(ServiceName, r.ServiceName)));
            Assert.AreEqual(testMessages.Count(), messages.Count);
            Assert.AreEqual(result.ElementAt(0).ID, messages[0].ID);
            Assert.AreEqual(result.ElementAt(1).ID, messages[1].ID);
            Assert.IsTrue(messages.TrueForAll(m =>
                Equals(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, m.Channel)
                && Equals(0, m.Header.Keys.Count())
                && Equals(Constants.BasicMessageType, m.MessageTypeID)
                && m.Data.Length > 0
            ));
            Assert.AreEqual(testMessages.ElementAt(0).message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(testMessages.ElementAt(1).message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[1].Data.ToArray())));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestBulkPublishAsyncWithNoBulkSupportAndDifferentChannelName()
        {
            #region Arrange
            var channelName = "MyTestChannel";
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            IEnumerable<(BasicMessage message, MessageHeader? messageHeader)> testMessages = [
                (new("testMessage"),null),
                (new("testMessage2"),null)
            ];

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MultiServiceInstance();
            contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channelName);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsTrue(result.All(r => r.Results.Count()==1));
            Assert.IsTrue(result.SelectMany(r => r.Results).All(r => !r.IsError && Equals(ServiceName, r.ServiceName)));
            Assert.AreEqual(testMessages.Count(), messages.Count);
            Assert.AreEqual(result.ElementAt(0).ID, messages[0].ID);
            Assert.AreEqual(result.ElementAt(1).ID, messages[1].ID);
            Assert.IsTrue(messages.TrueForAll(m =>
                Equals(channelName, m.Channel)
                && Equals(0, m.Header.Keys.Count())
                && Equals(Constants.BasicMessageType, m.MessageTypeID)
                && m.Data.Length > 0
            ));
            Assert.AreEqual(testMessages.ElementAt(0).message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(testMessages.ElementAt(1).message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[1].Data.ToArray())));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestBulkPublishAsyncWithNoBulkSupportAndMessageHeaders()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            IEnumerable<(BasicMessage message, MessageHeader? messageHeader)> testMessages = [
                (new("testMessage"),new MessageHeader([new KeyValuePair<string,string>("header1","headervalue1")])),
                (new("testMessage2"),new MessageHeader([new KeyValuePair<string,string>("header2","headervalue2")]))
            ];

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MultiServiceInstance();
            contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(r => r.Results.Count()==1));
            Assert.IsTrue(result.SelectMany(r => r.Results).All(r => !r.IsError && Equals(ServiceName, r.ServiceName)));
            Assert.AreEqual(testMessages.Count(), messages.Count);
            Assert.AreEqual(result.ElementAt(0).ID, messages[0].ID);
            Assert.AreEqual(result.ElementAt(1).ID, messages[1].ID);
            Assert.IsTrue(messages.TrueForAll(m =>
                Equals(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, m.Channel)
                && Equals(Constants.BasicMessageType, m.MessageTypeID)
                && m.Data.Length > 0
            ));
            Assert.IsTrue(testMessages.ElementAt(0).messageHeader?.Keys.All(k => Equals(messages[0].Header[k], testMessages.ElementAt(0).messageHeader?[k])));
            Assert.IsTrue(testMessages.ElementAt(1).messageHeader?.Keys.All(k => Equals(messages[1].Header[k], testMessages.ElementAt(1).messageHeader?[k])));
            Assert.AreEqual(testMessages.ElementAt(0).message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(testMessages.ElementAt(1).message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[1].Data.ToArray())));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestBulkPublishAsyncWithBulkSupport()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            IEnumerable<(BasicMessage message, MessageHeader? messageHeader)> testMessages = [
                (new("testMessage"),null),
                (new("testMessage2"),null)
            ];

            List<IEnumerable<ServiceMessage>> messages = [];

            var serviceConnection = new Mock<IBulkPublishableMessageServiceConnection>();
            serviceConnection.Setup(x => x.BulkPublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken) => ValueTask.FromResult(messages.Select(m => transmissionResult)));

            var contractConnection = ContractConnection.MultiServiceInstance();
            contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsTrue(result.All(r => r.Results.Count()==1));
            Assert.IsTrue(result.SelectMany(r => r.Results).All(r => !r.IsError && Equals(ServiceName, r.ServiceName)));
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(result.ElementAt(0).ID, messages[0].ElementAt(0).ID);
            Assert.AreEqual(result.ElementAt(1).ID, messages[0].ElementAt(1).ID);
            Assert.IsTrue(messages.SelectMany(messageSet => messageSet.Select(m => m)).All(m =>
                Equals(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, m.Channel)
                && Equals(0, m.Header.Keys.Count())
                && Equals(Constants.BasicMessageType, m.MessageTypeID)
                && m.Data.Length > 0
            ));
            Assert.AreEqual(testMessages.ElementAt(0).message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].ElementAt(0).Data.ToArray())));
            Assert.AreEqual(testMessages.ElementAt(1).message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].ElementAt(1).Data.ToArray())));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task TestBulkPublishAsyncWithNoBulkSupporttWithTelemetryData(bool withLinking)
        {
            #region Arrange
            (var listener, var capturedActivities, var sourceName) = ConnectionHelper.SetupTelemetry();
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            IEnumerable<(BasicMessage message, MessageHeader? messageHeader)> testMessages = [
                (new("testMessage"),null),
                (new("testMessage2"),null)
            ];

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: withLinking);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, testMessages.Count(), TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsTrue(result.All(r => r.Results.Count()==1));
            Assert.IsTrue(result.SelectMany(r => r.Results).All(r => !r.IsError && Equals(ServiceName, r.ServiceName)));
            Assert.AreEqual(testMessages.Count(), messages.Count);
            Assert.AreEqual(result.ElementAt(0).ID, messages[0].ID);
            Assert.AreEqual(result.ElementAt(1).ID, messages[1].ID);
            Assert.IsTrue(messages.TrueForAll(m =>
                Equals(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, m.Channel)
                && Equals((withLinking ? 2 : 0), m.Header.Keys.Count())
                && Equals(Constants.BasicMessageType, m.MessageTypeID)
                && m.Data.Length > 0
            ));
            Assert.AreEqual(testMessages.ElementAt(0).message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(testMessages.ElementAt(1).message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[1].Data.ToArray())));
            Assert.AreEqual(1, capturedActivities.Count);
            ConnectionHelper.ValidateBulkPublishActivity<BasicMessage>(
                messages,
                capturedActivities[0],
                "MQContract.BulkPublishMessages",
                serviceConnection.Object.GetType(),
                true,
                withLinking,
                false,
                connectionName: ServiceName
            );
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task TestBulkPublishAsyncWithBulkSupportWithTelemetryData(bool withLinking)
        {
            #region Arrange
            (var listener, var capturedActivities, var sourceName) = ConnectionHelper.SetupTelemetry();

            IEnumerable<(BasicMessage message, MessageHeader? messageHeader)> testMessages = [
                (new("testMessage"),null),
                (new("testMessage2"),null)
            ];

            List<IEnumerable<ServiceMessage>> messages = [];

            var serviceConnection = new Mock<IBulkPublishableMessageServiceConnection>();
            serviceConnection.Setup(x => x.BulkPublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken) => ValueTask.FromResult(messages.Select(m => new TransmissionResult(m.ID))));

            var contractConnection = ContractConnection.MultiServiceInstance();
            contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: withLinking);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsTrue(result.All(r => r.Results.Count()==1));
            Assert.IsTrue(result.SelectMany(r => r.Results).All(r => !r.IsError && Equals(ServiceName, r.ServiceName)));
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(result.ElementAt(0).ID, messages[0].ElementAt(0).ID);
            Assert.AreEqual(result.ElementAt(1).ID, messages[0].ElementAt(1).ID);
            Assert.IsTrue(messages.SelectMany(messageSet => messageSet.Select(m => m)).All(m =>
                Equals(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, m.Channel)
                && Equals((withLinking ? 2 : 0), m.Header.Keys.Count())
                && Equals(Constants.BasicMessageType, m.MessageTypeID)
                && m.Data.Length > 0
            ));
            Assert.AreEqual(testMessages.ElementAt(0).message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].ElementAt(0).Data.ToArray())));
            Assert.AreEqual(testMessages.ElementAt(1).message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].ElementAt(1).Data.ToArray())));
            ConnectionHelper.ValidateBulkPublishActivity<BasicMessage>(
                messages.SelectMany(m => m),
                capturedActivities[0],
                "MQContract.BulkPublishMessages",
                serviceConnection.Object.GetType(),
                true,
                withLinking,
                true,
                connectionName: ServiceName
            );
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicMessage), false)]
        public async Task TestBulkPublishAsyncWithNoBulkSupportAndRetryFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var error = new Exception("Failed");
            var retryCount = 2;

            IEnumerable<(BasicMessage message, MessageHeader? messageHeader)> testMessages = [
                (new("testMessage"),null),
                (new("testMessage2"),null)
            ];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsInOrder(
                    ValueTask.FromResult(transmissionResult),
                    ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), new(error, false))),
                    ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), new(error, false))),
                    ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), new(error, false)))
                );

            var serviceConnection2 = new Mock<IMessageServiceConnection>();
            serviceConnection2.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsInOrder(
                    ValueTask.FromResult(transmissionResult),
                    ValueTask.FromResult(transmissionResult)
                );

            var contractConnection = ContractConnection.MultiServiceInstance();
            contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object);
            contractConnection.RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, null);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, retryCount, null);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            var failed = result.Last().Results.First(r => Equals(r.ServiceName, ServiceName));
            Assert.IsTrue(failed.IsError);
            Assert.IsNotNull(failed.Error);
            Assert.IsInstanceOfType<ResilienceException>(failed.Error.Exception);
            Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)failed.Error.Exception).Type);
            Assert.IsNotNull(failed.Error.Exception.InnerException);
            Assert.AreEqual(error.Message, failed.Error.Exception.InnerException.Message);
            Assert.IsTrue(Array.TrueForAll(result.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName2))).ToArray(), r => !r.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+2));
            serviceConnection2.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicMessage), false)]
        public async Task TestBulkPublishAsyncWithNoBulkSupportAndCircuitBreakFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var error = new Exception("Failed");
            var circuitBreakCount = 1;

            IEnumerable<(BasicMessage message, MessageHeader? messageHeader)> testMessages = [
                (new("testMessage"),null),
                (new("testMessage2"),null)
            ];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsInOrder(
                    ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), new(error, false)))
                );

            var serviceConnection2 = new Mock<IMessageServiceConnection>();
            serviceConnection2.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsInOrder(
                    ValueTask.FromResult(transmissionResult),
                    ValueTask.FromResult(transmissionResult)
                );

            var contractConnection = ContractConnection.MultiServiceInstance();
            contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object);
            contractConnection.RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, null, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, null, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            var failed = result.Last().Results.First(r => Equals(r.ServiceName, ServiceName));
            Assert.IsTrue(failed.IsError);
            Assert.IsNotNull(failed.Error);
            Assert.IsInstanceOfType<ResilienceException>(failed.Error.Exception);
            Assert.AreEqual(ResilienceTypes.CircuitBreak, ((ResilienceException)failed.Error.Exception).Type);
            Assert.IsNotNull(failed.Error.Exception.InnerException);
            Assert.IsInstanceOfType<BrokenCircuitException>(failed.Error.Exception.InnerException);
            Assert.IsTrue(Array.TrueForAll(result.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName2))).ToArray(), r => !r.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(circuitBreakCount));
            serviceConnection2.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicMessage), false)]
        public async Task TestBulkPublishAsyncWithNoBulkWithRetryAndCircuitBreakFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var error = new Exception("Failed");
            var circuitBreakCount = 3;
            var retryCount = 2;

            IEnumerable<(BasicMessage message, MessageHeader? messageHeader)> testMessages = [
                (new("testMessage"),null),
                (new("testMessage2"),null)
            ];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsInOrder(
                    ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), new(error, false))),
                    ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), new(error, false))),
                    ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), new(error, false))),
                    ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), new(error, false)))
                );

            var serviceConnection2 = new Mock<IMessageServiceConnection>();
            serviceConnection2.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsInOrder(
                    ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString())),
                    ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString()))
                );

            var contractConnection = ContractConnection.MultiServiceInstance();
            contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object);
            contractConnection.RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            var retryFailure = result.First().Results.First(r => Equals(r.ServiceName, ServiceName));
            Assert.IsTrue(retryFailure.IsError);
            Assert.IsNotNull(retryFailure.Error);
            Assert.IsInstanceOfType<ResilienceException>(retryFailure.Error.Exception);
            Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)retryFailure.Error.Exception).Type);
            Assert.AreEqual(error, retryFailure.Error.Exception.InnerException);
            var circuitFailure = result.Last().Results.First(r => Equals(r.ServiceName, ServiceName));
            Assert.IsTrue(circuitFailure.IsError);
            Assert.IsNotNull(circuitFailure.Error);
            Assert.IsInstanceOfType<ResilienceException>(circuitFailure.Error.Exception);
            Assert.AreEqual(ResilienceTypes.CircuitBreak, ((ResilienceException)circuitFailure.Error.Exception).Type);
            Assert.IsNotNull(circuitFailure.Error.Exception.InnerException);
            Assert.IsInstanceOfType<BrokenCircuitException>(circuitFailure.Error.Exception.InnerException);
            Assert.IsTrue(Array.TrueForAll(result.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName2))).ToArray(), r => !r.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection2.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicMessage), false)]
        public async Task TestBulkPublishAsyncWithNoBulkWithRetryAndCircuitBreakWithoutFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var error = new Exception("Failed");
            var circuitBreakCount = 3;
            var retryCount = 2;

            IEnumerable<(BasicMessage message, MessageHeader? messageHeader)> testMessages = [
                (new("testMessage"),null),
                (new("testMessage2"),null)
            ];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsInOrder(
                    ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), new(error, true))),
                    ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString(), new(error, true)))
                );

            var serviceConnection2 = new Mock<IMessageServiceConnection>();
            serviceConnection2.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsInOrder(
                    ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString())),
                    ValueTask.FromResult(new TransmissionResult(Guid.NewGuid().ToString()))
                );

            var contractConnection = ContractConnection.MultiServiceInstance();
            contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object);
            contractConnection.RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(Array.TrueForAll(result.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName))).ToArray(), r => r.IsError && Equals(error, r.Error!.Exception) && r.Error!.IsFatal));
            Assert.IsTrue(Array.TrueForAll(result.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName2))).ToArray(), r => !r.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection2.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicMessage), false)]
        public async Task TestBulkPublishAsyncWithBulkSupportAndRetryFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var error = new Exception("Failed");
            var retryCount = 2;

            IEnumerable<(BasicMessage message, MessageHeader? messageHeader)> testMessages = [
                (new("testMessage"),null),
                (new("testMessage2"),null)
            ];

            var serviceConnection = new Mock<IBulkPublishableMessageServiceConnection>();
            serviceConnection.Setup(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken) =>
                {
                    if (serviceMessages.Count()==2)
                        return ValueTask.FromResult<IEnumerable<TransmissionResult>>([new(serviceMessages.First().ID), new(serviceMessages.Last().ID, new(error, false))]);
                    return ValueTask.FromResult(serviceMessages.Select(m => new TransmissionResult(m.ID, new(error, false))));
                });
            var serviceConnection2 = new Mock<IBulkPublishableMessageServiceConnection>();
            serviceConnection2.Setup(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken) =>
                {
                    return ValueTask.FromResult(serviceMessages.Select(m => new TransmissionResult(m.ID)));
                });

            var contractConnection = ContractConnection.MultiServiceInstance();
            contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object);
            contractConnection.RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, null);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, retryCount, null);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            var failed = result.Last().Results.First(r => Equals(ServiceName, r.ServiceName));
            Assert.IsTrue(failed.IsError);
            Assert.IsNotNull(failed.Error);
            Assert.IsInstanceOfType<ResilienceException>(failed.Error.Exception);
            Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)failed.Error.Exception).Type);
            Assert.IsNotNull(failed.Error.Exception.InnerException);
            Assert.AreEqual(error.Message, failed.Error.Exception.InnerException.Message);
            Assert.IsTrue(Array.TrueForAll(result.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName2))).ToArray(), r => !r.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection2.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicMessage), false)]
        public async Task TestBulkPublishAsyncWithBulkSupportAndCircuitBreakFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var error = new Exception("Failed");
            var circuitBreakCount = 1;

            IEnumerable<(BasicMessage message, MessageHeader? messageHeader)> testMessages = [
                (new("testMessage"),null),
                (new("testMessage2"),null)
            ];

            var serviceConnection = new Mock<IBulkPublishableMessageServiceConnection>();
            serviceConnection.Setup(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken) =>
                {
                    if (serviceMessages.Count()==2)
                        return ValueTask.FromResult<IEnumerable<TransmissionResult>>([new(serviceMessages.First().ID), new(serviceMessages.Last().ID, new(error, false))]);
                    return ValueTask.FromResult(serviceMessages.Select(m => new TransmissionResult(m.ID, new(error, false))));
                });
            var serviceConnection2 = new Mock<IBulkPublishableMessageServiceConnection>();
            serviceConnection2.Setup(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken) =>
                {
                    return ValueTask.FromResult(serviceMessages.Select(m => new TransmissionResult(m.ID)));
                });

            var contractConnection = ContractConnection.MultiServiceInstance();
            contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object);
            contractConnection.RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, null, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, null, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            _ = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel);
            var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            var failed = result.Last().Results.First(r => Equals(ServiceName, r.ServiceName));
            Assert.IsTrue(failed.IsError);
            Assert.IsNotNull(failed.Error);
            Assert.IsInstanceOfType<ResilienceException>(failed.Error.Exception);
            Assert.AreEqual(ResilienceTypes.CircuitBreak, ((ResilienceException)failed.Error.Exception).Type);
            Assert.IsNotNull(failed.Error.Exception.InnerException);
            Assert.IsInstanceOfType<BrokenCircuitException>(failed.Error.Exception.InnerException);
            Assert.IsTrue(Array.TrueForAll(result.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName2))).ToArray(), r => !r.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(circuitBreakCount));
            serviceConnection2.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicMessage), false)]
        public async Task TestBulkPublishAsyncWithBulkSupportWithRetryAndCircuitBreakFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var error = new Exception("Failed");
            var circuitBreakCount = 3;
            var retryCount = 2;

            IEnumerable<(BasicMessage message, MessageHeader? messageHeader)> testMessages = [
                (new("testMessage"),null),
                (new("testMessage2"),null)
            ];

            var serviceConnection = new Mock<IBulkPublishableMessageServiceConnection>();
            serviceConnection.Setup(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken) =>
                {
                    if (serviceMessages.Count()==2)
                        return ValueTask.FromResult<IEnumerable<TransmissionResult>>([new(serviceMessages.First().ID), new(serviceMessages.Last().ID, new(error, false))]);
                    return ValueTask.FromResult(serviceMessages.Select(m => new TransmissionResult(m.ID, new(error, false))));
                });
            var serviceConnection2 = new Mock<IBulkPublishableMessageServiceConnection>();
            serviceConnection2.Setup(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken) =>
                {
                    return ValueTask.FromResult(serviceMessages.Select(m => new TransmissionResult(m.ID)));
                });

            var contractConnection = ContractConnection.MultiServiceInstance();
            contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object);
            contractConnection.RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var retryResults = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel);
            var circuitBreakResults = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(retryResults);
            Assert.IsNotNull(circuitBreakResults);
            var retryFailure = retryResults.Last().Results.First(r => Equals(ServiceName, r.ServiceName));
            Assert.IsTrue(retryFailure.IsError);
            Assert.IsNotNull(retryFailure.Error);
            Assert.IsInstanceOfType<ResilienceException>(retryFailure.Error.Exception);
            Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)retryFailure.Error.Exception).Type);
            Assert.AreEqual(error, retryFailure.Error.Exception.InnerException);
            Assert.IsTrue(Array.TrueForAll(circuitBreakResults.SelectMany(result => result.Results.Where(r => Equals(ServiceName, r.ServiceName))).ToArray(), result => result.IsError
            && result.Error!=null
            && result.Error.Exception is ResilienceException re
            && Equals(ResilienceTypes.CircuitBreak, re.Type)
            && re.InnerException is BrokenCircuitException));
            Assert.IsTrue(Array.TrueForAll(retryResults.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName2))).ToArray(), r => !r.IsError));
            Assert.IsTrue(Array.TrueForAll(circuitBreakResults.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName2))).ToArray(), r => !r.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection2.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicMessage), false)]
        public async Task TestBulkPublishAsyncWithBulkSupportWithRetryAndCircuitBreakWithoutFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var error = new Exception("Failed");
            var circuitBreakCount = 3;
            var retryCount = 2;

            IEnumerable<(BasicMessage message, MessageHeader? messageHeader)> testMessages = [
                (new("testMessage"),null),
                (new("testMessage2"),null)
            ];

            List<IEnumerable<ServiceMessage>> messages = [];

            var serviceConnection = new Mock<IBulkPublishableMessageServiceConnection>();
            serviceConnection.Setup(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken) =>
                {
                    messages.Add(serviceMessages);
                    return ValueTask.FromResult(serviceMessages.Select(m => new TransmissionResult(m.ID, new(error, true))));
                });
            var serviceConnection2 = new Mock<IBulkPublishableMessageServiceConnection>();
            serviceConnection2.Setup(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken) =>
                {
                    return ValueTask.FromResult(serviceMessages.Select(m => new TransmissionResult(m.ID)));
                });

            var contractConnection = ContractConnection.MultiServiceInstance();
            contractConnection.RegisterServiceConnection(ServiceName, serviceConnection.Object);
            contractConnection.RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var retryResults = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel);
            var circuitBreakResults = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(retryResults);
            Assert.IsNotNull(circuitBreakResults);
            Assert.IsTrue(Array.TrueForAll(retryResults.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName))).ToArray(), r => r.IsError && Equals(error, r.Error!.Exception) && r.Error!.IsFatal));
            Assert.IsNotNull(circuitBreakResults);
            Assert.AreEqual(testMessages.Count(), circuitBreakResults.Count());
            Assert.IsTrue(Array.TrueForAll(circuitBreakResults.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName))).ToArray(), r => r.IsError && Equals(error, r.Error!.Exception) && r.Error!.IsFatal));
            Assert.IsTrue(Array.TrueForAll(retryResults.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName2))).ToArray(), r => !r.IsError));
            Assert.IsTrue(Array.TrueForAll(circuitBreakResults.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName2))).ToArray(), r => !r.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection2.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestBulkPublishAsyncWithBulkSupportWithRetryAndCircuitBreakFailureAndEnsuringPriority()
        {
            #region Arrange
            var serviceName3 = "testService3";
            var channel = "testChannel";
            var messageType = typeof(BasicMessage);
            var error = new Exception("Failed");
            var circuitBreakCount = 2;
            var retryCount = 1;

            IEnumerable<(BasicMessage message, MessageHeader? messageHeader)> testMessages = [
                (new("testMessage"),null),
                (new("testMessage2"),null)
            ];

            var serviceConnection = new Mock<IBulkPublishableMessageServiceConnection>();
            serviceConnection.Setup(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken) =>
                {
                    if (serviceMessages.Count()==2)
                        return ValueTask.FromResult<IEnumerable<TransmissionResult>>([new(serviceMessages.First().ID), new(serviceMessages.Last().ID, new(error, false))]);
                    return ValueTask.FromResult(serviceMessages.Select(m => new TransmissionResult(m.ID, new(error, false))));
                });
            var serviceConnection2 = new Mock<IBulkPublishableMessageServiceConnection>();
            serviceConnection2.Setup(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken) =>
                {
                    return ValueTask.FromResult(serviceMessages.Select(m => new TransmissionResult(m.ID)));
                });
            var serviceConnection3 = new Mock<IBulkPublishableMessageServiceConnection>();
            serviceConnection3.Setup(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken) =>
                {
                    if (serviceMessages.Count()==2)
                        return ValueTask.FromResult<IEnumerable<TransmissionResult>>([new(serviceMessages.First().ID), new(serviceMessages.Last().ID, new(error, false))]);
                    return ValueTask.FromResult(serviceMessages.Select(m => new TransmissionResult(m.ID, new(error, false))));
                });

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .RegisterServiceConnection(ServiceName2, serviceConnection2.Object)
                .RegisterServiceConnection(serviceName3, serviceConnection3.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, null, false, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, null, messageType, false, retryCount+1, circuitBreakCount+1);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName2, channel, null, false, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName2, null, messageType, false, retryCount+1, circuitBreakCount+1);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, null, channel, null, false, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, null, null, messageType, false, retryCount+1, circuitBreakCount+1);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var channelRetryResults = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel);
            var channelCircuitBreakResults = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel);
            var typeRetryResults = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages);
            var typeCircuitBreakResults = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(channelRetryResults);
            Assert.IsNotNull(channelCircuitBreakResults);
            Assert.AreEqual(testMessages.Count(), channelRetryResults.Count());
            Assert.AreEqual(testMessages.Count(), channelCircuitBreakResults.Count());
            Assert.IsTrue(Array.TrueForAll(channelRetryResults.Last().Results.Where(r => Equals(ServiceName, r.ServiceName) || Equals(serviceName3, r.ServiceName)).ToArray(),
                retryFailure => retryFailure.IsError
                && retryFailure.Error!=null
                && retryFailure.Error.Exception is ResilienceException resilienceException
                && Equals(ResilienceTypes.Retry, resilienceException.Type)
                && Equals(error, resilienceException.InnerException)
            ));
            Assert.IsTrue(Array.TrueForAll(channelCircuitBreakResults.SelectMany(result => result.Results.Where(r => Equals(ServiceName, r.ServiceName))).ToArray(), result => result.IsError
            && result.Error!=null
            && result.Error.Exception is ResilienceException re
            && Equals(ResilienceTypes.CircuitBreak, re.Type)
            && re.InnerException is BrokenCircuitException));
            Assert.IsNotNull(typeRetryResults);
            Assert.IsNotNull(typeCircuitBreakResults);
            Assert.AreEqual(testMessages.Count(), typeRetryResults.Count());
            Assert.AreEqual(testMessages.Count(), typeCircuitBreakResults.Count());
            Assert.IsTrue(Array.TrueForAll(typeRetryResults.Last().Results.Where(r => Equals(ServiceName, r.ServiceName) || Equals(serviceName3, r.ServiceName)).ToArray(),
                retryFailure => retryFailure.IsError
                && retryFailure.Error!=null
                && retryFailure.Error.Exception is ResilienceException resilienceException
                && Equals(ResilienceTypes.Retry, resilienceException.Type)
                && Equals(error, resilienceException.InnerException)
            ));
            Assert.IsTrue(Array.TrueForAll(typeCircuitBreakResults.SelectMany(result => result.Results.Where(r => Equals(ServiceName, r.ServiceName))).ToArray(), result => result.IsError
            && result.Error!=null
            && result.Error.Exception is ResilienceException re
            && Equals(ResilienceTypes.CircuitBreak, re.Type)
            && re.InnerException is BrokenCircuitException));

            Assert.IsTrue(Array.TrueForAll(channelRetryResults.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName2))).ToArray(), r => !r.IsError));
            Assert.IsTrue(Array.TrueForAll(channelCircuitBreakResults.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName2))).ToArray(), r => !r.IsError));
            Assert.IsTrue(Array.TrueForAll(typeRetryResults.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName2))).ToArray(), r => !r.IsError));
            Assert.IsTrue(Array.TrueForAll(typeCircuitBreakResults.SelectMany(r => r.Results.Where(r => Equals(r.ServiceName, ServiceName2))).ToArray(), r => !r.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(((retryCount+1)*2)+1));
            serviceConnection2.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
            serviceConnection3.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(((retryCount+1)*2)+1));
            #endregion
        }
    }
}
