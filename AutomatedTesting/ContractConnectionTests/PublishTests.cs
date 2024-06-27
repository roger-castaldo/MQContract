using Moq;
using MQContract.ServiceAbstractions.Messages;
using MQContract.ServiceAbstractions;
using MQContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using AutomatedTesting.Messages;
using System.Text.Json;
using System.Diagnostics;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using System.Reflection;
using MQContract.Attributes;

namespace AutomatedTesting.ContractConnectionTests
{
    [TestClass]
    public class PublishTests
    {
        [TestMethod]
        public async Task TestPublishAsyncWithNoExtendedAspects()
        {
            #region Arrange
            var transmissionResult = new Mock<ITransmissionResult>();
            transmissionResult.Setup(x=>x.IsError)
                .Returns(false);
            transmissionResult.Setup(x => x.Error)
                .Returns("");
            transmissionResult.Setup(x => x.MessageID)
                .Returns(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<IServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<IServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult.Object);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult.Object.MessageID, result.MessageID);
            Assert.AreEqual(transmissionResult.Object.Error, result.Error);
            Assert.AreEqual(transmissionResult.Object.IsError, result.IsError);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name,messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0,messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, JsonSerializer.Deserialize<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithDifferentChannelName()
        {
            #region Arrange
            var transmissionResult = new Mock<ITransmissionResult>();
            transmissionResult.Setup(x => x.IsError)
                .Returns(false);
            transmissionResult.Setup(x => x.Error)
                .Returns("");
            transmissionResult.Setup(x => x.MessageID)
                .Returns(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");

            List<IServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<IServiceMessage>(messages), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult.Object);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage,channel:$"Not{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}");
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult.Object.MessageID, result.MessageID);
            Assert.AreEqual(transmissionResult.Object.Error, result.Error);
            Assert.AreEqual(transmissionResult.Object.IsError, result.IsError);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual($"Not{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}", messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, JsonSerializer.Deserialize<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithMessageHeaders()
        {
            #region Arrange
            var transmissionResult = new Mock<ITransmissionResult>();
            transmissionResult.Setup(x => x.IsError)
                .Returns(false);
            transmissionResult.Setup(x => x.Error)
                .Returns("");
            transmissionResult.Setup(x => x.MessageID)
                .Returns(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");

            List<IServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<IServiceMessage>(messages), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult.Object);

            var messageHeader = new Mock<IMessageHeader>(); ;
            messageHeader.Setup(x => x.Keys)
                .Returns(["testing"]);
            messageHeader.Setup(x => x["testing"])
                .Returns("testing");

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage,messageHeader:messageHeader.Object);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult.Object.MessageID, result.MessageID);
            Assert.AreEqual(transmissionResult.Object.Error, result.Error);
            Assert.AreEqual(transmissionResult.Object.IsError, result.IsError);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual("U-BasicMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, JsonSerializer.Deserialize<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(messageHeader.Object.Keys.Count(), messages[0].Header.Keys.Count());
            Assert.IsTrue(messageHeader.Object.Keys.All(k => messages[0].Header.Keys.Contains(k)));
            Assert.IsTrue(messageHeader.Object.Keys.All(k => Equals(messages[0].Header[k],messageHeader.Object[k])));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithTimeout()
        {
            #region Arrange
            var transmissionResult = new Mock<ITransmissionResult>();
            transmissionResult.Setup(x => x.IsError)
                .Returns(false);
            transmissionResult.Setup(x => x.Error)
                .Returns("");
            transmissionResult.Setup(x => x.MessageID)
                .Returns(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var timeout = TimeSpan.FromDays(1);

            List<IServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<IServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult.Object);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage,timeout:timeout);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult.Object.MessageID, result.MessageID);
            Assert.AreEqual(transmissionResult.Object.Error, result.Error);
            Assert.AreEqual(transmissionResult.Object.IsError, result.IsError);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, JsonSerializer.Deserialize<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(timeout, timeouts[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithGlobalEncoder()
        {
            #region Arrange
            var transmissionResult = new Mock<ITransmissionResult>();
            transmissionResult.Setup(x => x.IsError)
                .Returns(false);
            transmissionResult.Setup(x => x.Error)
                .Returns("");
            transmissionResult.Setup(x => x.MessageID)
                .Returns(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var encodedData = ASCIIEncoding.ASCII.GetBytes(testMessage.Name);

            List<IServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<IServiceMessage>(messages), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult.Object);

            var globalEncoder = new Mock<IMessageEncoder>();
            globalEncoder.Setup(x => x.Encode<BasicMessage>(It.IsAny<BasicMessage>()))
                .Returns(encodedData);

            var contractConnection = new ContractConnection(serviceConnection.Object,defaultMessageEncoder:globalEncoder.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult.Object.MessageID, result.MessageID);
            Assert.AreEqual(transmissionResult.Object.Error, result.Error);
            Assert.AreEqual(transmissionResult.Object.IsError, result.IsError);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("BasicMessage", messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(Convert.ToBase64String(encodedData), Convert.ToBase64String(messages[0].Data.ToArray()));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            globalEncoder.Verify(x => x.Encode<BasicMessage>(It.IsAny<BasicMessage>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithGlobalEncryptor()
        {
            #region Arrange
            var transmissionResult = new Mock<ITransmissionResult>();
            transmissionResult.Setup(x => x.IsError)
                .Returns(false);
            transmissionResult.Setup(x => x.Error)
                .Returns("");
            transmissionResult.Setup(x => x.MessageID)
                .Returns(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");

            List<IServiceMessage> messages = [];
            List<byte[]> binaries = [];
            Dictionary<string, string?> headers = new([
                    new KeyValuePair<string,string?>("test","test")
                ]);

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<IServiceMessage>(messages), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult.Object);

            var globalEncryptor = new Mock<IMessageEncryptor>();
            globalEncryptor.Setup(x => x.Encrypt(Capture.In<byte[]>(binaries),out headers))
                .Returns((byte[] binary,Dictionary<string,string?> h)=>binary.Reverse().ToArray());

            var contractConnection = new ContractConnection(serviceConnection.Object, defaultMessageEncryptor: globalEncryptor.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult.Object.MessageID, result.MessageID);
            Assert.AreEqual(transmissionResult.Object.Error, result.Error);
            Assert.AreEqual(transmissionResult.Object.IsError, result.IsError);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("BasicMessage", messages[0].Channel);
            Assert.AreEqual("U-BasicMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(Convert.ToBase64String(binaries[0].Reverse().ToArray()), Convert.ToBase64String(messages[0].Data.ToArray()));
            Assert.AreEqual(headers.Count, messages[0].Header.Keys.Count());
            Assert.IsTrue(headers.Keys.All(k => messages[0].Header.Keys.Contains(k)));
            Assert.AreEqual(headers[headers.Keys.First()], messages[0].Header[headers.Keys.First()]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            globalEncryptor.Verify(x => x.Encrypt(It.IsAny<byte[]>(), out headers), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithTimeoutAttribute()
        {
            #region Arrange
            var transmissionResult = new Mock<ITransmissionResult>();
            transmissionResult.Setup(x => x.IsError)
                .Returns(false);
            transmissionResult.Setup(x => x.Error)
                .Returns("");
            transmissionResult.Setup(x => x.MessageID)
                .Returns(Guid.NewGuid().ToString());

            var testMessage = new TimeoutMessage("testMessage");
            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<IServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<IServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult.Object);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<TimeoutMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult.Object.MessageID, result.MessageID);
            Assert.AreEqual(transmissionResult.Object.Error, result.Error);
            Assert.AreEqual(transmissionResult.Object.IsError, result.IsError);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(typeof(TimeoutMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(TimeSpan.FromMilliseconds(typeof(TimeoutMessage).GetCustomAttribute<MessageResponseTimeoutAttribute>(false)?.Value??0), timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-TimeoutMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, JsonSerializer.Deserialize<TimeoutMessage>(new MemoryStream(messages[0].Data.ToArray())));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithServiceChannelOptions()
        {
            #region Arrange
            var transmissionResult = new Mock<ITransmissionResult>();
            transmissionResult.Setup(x => x.IsError)
                .Returns(false);
            transmissionResult.Setup(x => x.Error)
                .Returns("");
            transmissionResult.Setup(x => x.MessageID)
                .Returns(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var defaultTimeout = TimeSpan.FromMinutes(1);
            var serviceChannelOptions = new TestServiceChannelOptions("PublishAsync");

            List<IServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];
            List<IServiceChannelOptions> options = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<IServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), Capture.In<IServiceChannelOptions>(options), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult.Object);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage,options:serviceChannelOptions);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult.Object.MessageID, result.MessageID);
            Assert.AreEqual(transmissionResult.Object.Error, result.Error);
            Assert.AreEqual(transmissionResult.Object.IsError, result.IsError);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, JsonSerializer.Deserialize<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(1, options.Count);
            Assert.IsInstanceOfType<TestServiceChannelOptions>(options[0]);
            Assert.AreEqual(serviceChannelOptions, options[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }
    }
}
