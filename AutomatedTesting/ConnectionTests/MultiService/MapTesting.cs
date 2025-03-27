using AutomatedTesting.Messages;
using Moq;
using MQContract;
using MQContract.Interfaces.Service;
using System.Diagnostics;

namespace AutomatedTesting.ConnectionTests.MultiService
{
    [TestClass]
    public class MapTesting
    {
        [TestMethod]
        public async Task TestMapByChannelName()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var serviceName = "myService";
            var otherServiceName = "myOtherService";
            var channelName = "testChannel";

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            var otherServiceConnection = new Mock<IMessageServiceConnection>();
            otherServiceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(channelName, serviceName, serviceConnection.Object)
                .RegisterServiceConnection($"Not{channelName}", otherServiceName, otherServiceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channelName);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(result.ID, messages[0].ID);
            Assert.AreEqual(1, result.Results.Count());
            Assert.AreEqual(serviceName, result.Results.First().ServiceName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            otherServiceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestMapByMessageType()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var serviceName = "myService";
            var otherServiceName = "myOtherService";

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            var otherServiceConnection = new Mock<IMessageServiceConnection>();
            otherServiceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(typeof(BasicMessage), serviceName, serviceConnection.Object)
                .RegisterServiceConnection(typeof(BasicQueryMessage), otherServiceName, otherServiceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(result.ID, messages[0].ID);
            Assert.AreEqual(1, result.Results.Count());
            Assert.AreEqual(serviceName, result.Results.First().ServiceName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            otherServiceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestMapByMessageTypeThroughGenerics()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var serviceName = "myService";
            var otherServiceName = "myOtherService";

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            var otherServiceConnection = new Mock<IMessageServiceConnection>();
            otherServiceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection<BasicMessage>(serviceName, serviceConnection.Object)
                .RegisterServiceConnection<BasicQueryMessage>(otherServiceName, otherServiceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(result.ID, messages[0].ID);
            Assert.AreEqual(1, result.Results.Count());
            Assert.AreEqual(serviceName, result.Results.First().ServiceName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            otherServiceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestMapByMessageHeader()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var serviceName = "myService";
            var otherServiceName = "myOtherService";
            var headerKey = "testHeader";
            var headerValue = "testValue";

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            var otherServiceConnection = new Mock<IMessageServiceConnection>();
            otherServiceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(headerKey, headerValue, serviceName, serviceConnection.Object)
                .RegisterServiceConnection(headerKey, $"Not{headerValue}", otherServiceName, otherServiceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, messageHeader: new([new(headerKey, headerValue)]));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(result.ID, messages[0].ID);
            Assert.AreEqual(1, result.Results.Count());
            Assert.AreEqual(serviceName, result.Results.First().ServiceName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            otherServiceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }
    }
}
