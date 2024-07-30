using AutomatedTesting.Encoders;
using AutomatedTesting.Encryptors;
using AutomatedTesting.Messages;
using AutomatedTesting.ServiceInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MQContract;
using MQContract.Attributes;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Service;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AutomatedTesting.ContractConnectionTests
{
    [TestClass]
    public class PublishTests
    {
        [TestMethod]
        public async Task TestPublishAsyncWithNoExtendedAspects()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
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
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithDifferentChannelName()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: $"Not{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}");
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual($"Not{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}", messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, JsonSerializer.Deserialize<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithMessageHeaders()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var messageHeader = new Mock<IMessageHeader>(); ;
            messageHeader.Setup(x => x.Keys)
                .Returns(["testing"]);
            messageHeader.Setup(x => x["testing"])
                .Returns("testing");

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, messageHeader: messageHeader.Object);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual("U-BasicMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, JsonSerializer.Deserialize<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(messageHeader.Object.Keys.Count(), messages[0].Header.Keys.Count());
            Assert.IsTrue(messageHeader.Object.Keys.All(k => messages[0].Header.Keys.Contains(k)));
            Assert.IsTrue(messageHeader.Object.Keys.All(k => Equals(messages[0].Header[k], messageHeader.Object[k])));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithTimeout()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var timeout = TimeSpan.FromDays(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, timeout: timeout);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, JsonSerializer.Deserialize<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(timeout, timeouts[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithCompressionDueToMessageSize()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("AAAAAAAAAAAAAAAAAAAaaaaaaaaaaaaaaaaaaaa");
            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);
            serviceConnection.Setup(x => x.MaxMessageBodySize)
                .Returns(35);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("C-BasicMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(
                new GZipStream(new MemoryStream(messages[0].Data.ToArray()), CompressionMode.Decompress)
            ));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithGlobalEncoder()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var encodedData = ASCIIEncoding.ASCII.GetBytes(testMessage.Name);

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var globalEncoder = new Mock<IMessageEncoder>();
            globalEncoder.Setup(x => x.Encode<BasicMessage>(It.IsAny<BasicMessage>()))
                .Returns(encodedData);

            var contractConnection = new ContractConnection(serviceConnection.Object, defaultMessageEncoder: globalEncoder.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual("BasicMessage", messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(Convert.ToBase64String(encodedData), Convert.ToBase64String(messages[0].Data.ToArray()));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            globalEncoder.Verify(x => x.Encode<BasicMessage>(It.IsAny<BasicMessage>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithGlobalEncryptor()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");

            List<ServiceMessage> messages = [];
            List<byte[]> binaries = [];
            Dictionary<string, string?> headers = new([
                    new KeyValuePair<string,string?>("test","test")
                ]);

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var globalEncryptor = new Mock<IMessageEncryptor>();
            globalEncryptor.Setup(x => x.Encrypt(Capture.In<byte[]>(binaries), out headers))
                .Returns((byte[] binary, Dictionary<string, string?> h) => binary.Reverse().ToArray());

            var contractConnection = new ContractConnection(serviceConnection.Object, defaultMessageEncryptor: globalEncryptor.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual("BasicMessage", messages[0].Channel);
            Assert.AreEqual("U-BasicMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(Convert.ToBase64String(binaries[0].Reverse().ToArray()), Convert.ToBase64String(messages[0].Data.ToArray()));
            Assert.AreEqual(headers.Count, messages[0].Header.Keys.Count());
            Assert.IsTrue(headers.Keys.All(k => messages[0].Header.Keys.Contains(k)));
            Assert.AreEqual(headers[headers.Keys.First()], messages[0].Header[headers.Keys.First()]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            globalEncryptor.Verify(x => x.Encrypt(It.IsAny<byte[]>(), out headers), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithTimeoutAttribute()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new TimeoutMessage("testMessage");
            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
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
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(TimeoutMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(TimeSpan.FromMilliseconds(typeof(TimeoutMessage).GetCustomAttribute<MessageResponseTimeoutAttribute>(false)?.Value??0), timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-TimeoutMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, JsonSerializer.Deserialize<TimeoutMessage>(new MemoryStream(messages[0].Data.ToArray())));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithServiceChannelOptions()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var defaultTimeout = TimeSpan.FromMinutes(1);
            var serviceChannelOptions = new TestServiceChannelOptions("PublishAsync");

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];
            List<IServiceChannelOptions> options = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), Capture.In<IServiceChannelOptions>(options), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, options: serviceChannelOptions);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
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
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithNamedAndVersionedMessage()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new NamedAndVersionedMessage("testMessage");
            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<NamedAndVersionedMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual($"U-{typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageNameAttribute>(false)?.Value}-{typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageVersionAttribute>(false)?.Version}", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, JsonSerializer.Deserialize<NamedAndVersionedMessage>(new MemoryStream(messages[0].Data.ToArray())));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithMessageWithDefinedEncoder()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new CustomEncoderMessage("testMessage");
            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<CustomEncoderMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(CustomEncoderMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-CustomEncoderMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, new TestMessageEncoder().Decode(new MemoryStream(messages[0].Data.ToArray())));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithMessageWithDefinedServiceInjectableEncoder()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());


            var testMessage = new CustomEncoderWithInjectionMessage("testMessage");
            var defaultTimeout = TimeSpan.FromMinutes(1);
            var serviceName = "TestPublishAsyncWithMessageWithDefinedServiceInjectableEncoder";
            var services = Helper.ProduceServiceProvider(serviceName);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object, serviceProvider: services);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<CustomEncoderWithInjectionMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(CustomEncoderWithInjectionMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-CustomEncoderWithInjectionMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage,
                new TestMessageEncoderWithInjection(services.GetRequiredService<IInjectableService>()).Decode(new MemoryStream(messages[0].Data.ToArray()))
            );
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithMessageWithDefinedEncryptor()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new CustomEncryptorMessage("testMessage");
            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<CustomEncryptorMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(CustomEncryptorMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual("U-CustomEncryptorMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            var decodedData = new TestMessageEncryptor().Decrypt(new MemoryStream(messages[0].Data.ToArray()), messages[0].Header);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<CustomEncryptorMessage>(decodedData));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithMessageWithDefinedServiceInjectableEncryptor()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new CustomEncryptorWithInjectionMessage("testMessage");
            var defaultTimeout = TimeSpan.FromMinutes(1);
            var serviceName = "TestPublishAsyncWithMessageWithDefinedServiceInjectableEncryptor";
            var services = Helper.ProduceServiceProvider(serviceName);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object, serviceProvider: services);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<CustomEncryptorWithInjectionMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(CustomEncryptorWithInjectionMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual("U-CustomEncryptorWithInjectionMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            var decodedData = new TestMessageEncryptorWithInjection(services.GetRequiredService<IInjectableService>()).Decrypt(new MemoryStream(messages[0].Data.ToArray()), messages[0].Header);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<CustomEncryptorWithInjectionMessage>(decodedData));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithNoMessageChannelThrowsError()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new NoChannelMessage("testMessage");
            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var exception = await Assert.ThrowsExceptionAsync<MessageChannelNullException>(() => contractConnection.PublishAsync<NoChannelMessage>(testMessage));
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual("message must have a channel value (Parameter 'channel')", exception.Message);
            Assert.AreEqual("channel", exception.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithToLargeAMessageThrowsError()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);
            serviceConnection.Setup(x => x.MaxMessageBodySize)
                .Returns(1);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var exception = await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => contractConnection.PublishAsync<BasicMessage>(testMessage));
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.StartsWith($"message data exceeds maxmium message size (MaxSize:{serviceConnection.Object.MaxMessageBodySize},"));
            Assert.AreEqual("message", exception.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithTwoDifferentMessageTypes()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage1 = new BasicMessage("testMessage1");
            var testMessage2 = new NoChannelMessage("testMessage2");
            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.PublishAsync<BasicMessage>(testMessage1);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Start();
            var result2 = await contractConnection.PublishAsync<NoChannelMessage>(testMessage2, channel: "TestChannel2");
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.AreEqual(transmissionResult, result1);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(2, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage1, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));

            Assert.IsNotNull(result2);
            Assert.AreEqual(transmissionResult, result2);
            Assert.AreEqual("TestChannel2", messages[1].Channel);
            Assert.AreEqual(defaultTimeout, timeouts[1]);
            Assert.AreEqual(0, messages[1].Header.Keys.Count());
            Assert.AreEqual("U-NoChannelMessage-0.0.0.0", messages[1].MessageTypeID);
            Assert.IsTrue(messages[1].Data.Length>0);
            Assert.AreEqual(testMessage2, await JsonSerializer.DeserializeAsync<NoChannelMessage>(new MemoryStream(messages[1].Data.ToArray())));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }
    }
}
