using AutomatedTesting.Messages;
using Moq;
using MQContract;
using MQContract.Attributes;
using MQContract.Interfaces.Service;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace AutomatedTesting.ConnectionTests.SingleService
{
    [TestClass]
    public class BulkPublishTests
    {
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

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
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
            Assert.IsTrue(result.All(r => Equals(r, transmissionResult)));
            Assert.AreEqual(testMessages.Count(), messages.Count);
            Assert.IsTrue(messages.TrueForAll(m =>
                Equals(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, m.Channel)
                && Equals(0, m.Header.Keys.Count())
                && Equals("U-BasicMessage-0.0.0.0", m.MessageTypeID)
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

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
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
            Assert.IsTrue(result.All(r => Equals(r, transmissionResult)));
            Assert.AreEqual(2, messages.Count);
            Assert.IsTrue(messages.TrueForAll(m =>
                Equals(channelName, m.Channel)
                && Equals(0, m.Header.Keys.Count())
                && Equals("U-BasicMessage-0.0.0.0", m.MessageTypeID)
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

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
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
            Assert.IsTrue(result.All(r => Equals(r, transmissionResult)));
            Assert.AreEqual(2, messages.Count);
            Assert.IsTrue(messages.TrueForAll(m =>
                Equals(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, m.Channel)
                && Equals("U-BasicMessage-0.0.0.0", m.MessageTypeID)
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

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
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
            Assert.IsTrue(result.All(r => Equals(r, transmissionResult)));
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(messages.SelectMany(messageSet => messageSet.Select(m => m)).All(m =>
                Equals(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, m.Channel)
                && Equals(0, m.Header.Keys.Count())
                && Equals("U-BasicMessage-0.0.0.0", m.MessageTypeID)
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
        public async Task TestBulkPublishAsyncWithNoBulkSupportWithTelemetryDataWithoutLinking()
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

            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .EnableOpenTelemetry(activitySource:sourceName, linkActivitiesAcrossSystems: false);
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
            Assert.IsTrue(result.All(r => Equals(r, transmissionResult)));
            Assert.AreEqual(testMessages.Count(), messages.Count);
            Assert.IsTrue(messages.TrueForAll(m =>
                Equals(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, m.Channel)
                && Equals(0, m.Header.Keys.Count())
                && Equals("U-BasicMessage-0.0.0.0", m.MessageTypeID)
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
                false,
                false
            );
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestBulkPublishAsyncWithNoBulkSupportWithTelemetryDataWithLinking()
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

            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: true);
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
            Assert.IsTrue(result.All(r => Equals(r, transmissionResult)));
            Assert.AreEqual(testMessages.Count(), messages.Count);
            Assert.IsTrue(messages.TrueForAll(m =>
                Equals(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, m.Channel)
                && Equals(2, m.Header.Keys.Count())
                && Equals("U-BasicMessage-0.0.0.0", m.MessageTypeID)
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
                true,
                false
            );
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestBulkPublishAsyncWithBulkSupportWithTelemetryDataWithoutLinking()
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

            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: false);
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
            Assert.IsTrue(result.All(r => messages.Any(m=>m.Any(m=>Equals(r.ID,m.ID)))));
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(messages.SelectMany(messageSet => messageSet.Select(m => m)).All(m =>
                Equals(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, m.Channel)
                && Equals(0, m.Header.Keys.Count())
                && Equals("U-BasicMessage-0.0.0.0", m.MessageTypeID)
                && m.Data.Length > 0
            ));
            Assert.AreEqual(testMessages.ElementAt(0).message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].ElementAt(0).Data.ToArray())));
            Assert.AreEqual(testMessages.ElementAt(1).message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].ElementAt(1).Data.ToArray())));
            ConnectionHelper.ValidateBulkPublishActivity<BasicMessage>(
                messages.SelectMany(m=>m),
                capturedActivities[0],
                "MQContract.BulkPublishMessages",
                serviceConnection.Object.GetType(),
                true,
                false,
                true
            );
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestBulkPublishAsyncWithBulkSupportWithTelemetryDataWithLinking()
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

            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: true);
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
            Assert.IsTrue(result.All(r => messages.Any(m => m.Any(m => Equals(r.ID, m.ID)))));
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(messages.SelectMany(messageSet => messageSet.Select(m => m)).All(m =>
                Equals(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, m.Channel)
                && Equals(2, m.Header.Keys.Count())
                && Equals("U-BasicMessage-0.0.0.0", m.MessageTypeID)
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
                true,
                true
            );
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }
    }
}
