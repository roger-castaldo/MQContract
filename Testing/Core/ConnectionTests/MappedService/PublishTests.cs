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
using Polly.CircuitBreaker;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AutomatedTesting.ConnectionTests.MappedService
{
    [TestClass]
    public class PublishTests
    {
        private const string ServiceName = "testService";
        [TestMethod]
        public async Task TestPublishAsyncWithNoExtendedAspects()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
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
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: $"Not{typeof(BasicMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel}", cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual($"Not{typeof(BasicMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel}", messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
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
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var messageHeader = new MessageHeader([new("testing", "testing")]);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, messageHeader: messageHeader, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.AreEqual(Constants.BasicMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: TestContext.CancellationToken));
            Assert.AreEqual(messageHeader.Keys.Count(), messages[0].Header.Keys.Count());
            Assert.IsTrue(messageHeader.Keys.All(k => messages[0].Header.Keys.Contains(k)));
            Assert.IsTrue(messageHeader.Keys.All(k => Equals(messages[0].Header[k], messageHeader[k])));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithCompressionDueToMessageSize()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("AAAAAAAAAAAAAAAAAAAaaaaaaaaaaaaaaaaaaaa");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            serviceConnection.Setup(x => x.MaxMessageBodySize)
                .Returns(35);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.AreEqual(1, messages[0].Header.Keys.Count());
            Assert.AreEqual("true", messages[0].Header[messages[0].Header.Keys.First()]);
            Assert.AreEqual(Constants.BasicMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(
                new GZipStream(new MemoryStream(messages[0].Data.ToArray()), CompressionMode.Decompress)
, cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithGlobalEncoder()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var encodedData = Encoding.ASCII.GetBytes(testMessage.Name);

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var globalEncoder = new Mock<IMessageEncoder>();
            globalEncoder.Setup(x => x.EncodeAsync(It.IsAny<BasicMessage>()))
                .ReturnsAsync(encodedData);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, defaultMessageEncoder: globalEncoder.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual("BasicMessage", messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(Convert.ToBase64String(encodedData), Convert.ToBase64String(messages[0].Data.ToArray()));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            globalEncoder.Verify(x => x.EncodeAsync(It.IsAny<BasicMessage>()), Times.Once);
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
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var globalEncryptor = new Mock<IMessageEncryptor>();
            globalEncryptor.Setup(x => x.EncryptAsync(Capture.In(binaries)))
                .ReturnsAsync((byte[] binary) => new(headers,binary.Reverse().ToArray()));

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, defaultMessageEncryptor: globalEncryptor.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual("BasicMessage", messages[0].Channel);
            Assert.AreEqual(Constants.BasicMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(Convert.ToBase64String(binaries[0].Reverse().ToArray()), Convert.ToBase64String(messages[0].Data.ToArray()));
            Assert.AreEqual(headers.Count, messages[0].Header.Keys.Count());
            Assert.IsTrue(headers.Keys.All(k => messages[0].Header.Keys.Contains(k)));
            Assert.AreEqual(headers[headers.Keys.First()], messages[0].Header[headers.Keys.First()]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            globalEncryptor.Verify(x => x.EncryptAsync(It.IsAny<byte[]>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithNamedAndVersionedMessage()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new NamedAndVersionedMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<NamedAndVersionedMessage>(testMessage, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.NamedAndVersionedMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<NamedAndVersionedMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithMessageWithDefinedEncoder()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new CustomEncoderMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<CustomEncoderMessage>(testMessage, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(CustomEncoderMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.CustomEncoderMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await new TestMessageEncoder().DecodeAsync(new MemoryStream(messages[0].Data.ToArray())));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithMessageWithDefinedServiceInjectableEncoder()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());


            var testMessage = new CustomEncoderWithInjectionMessage("testMessage");
            var serviceName = "TestPublishAsyncWithMessageWithDefinedServiceInjectableEncoder";
            var services = Helper.ProduceServiceProvider(serviceName);

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, serviceProvider: services);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<CustomEncoderWithInjectionMessage>(testMessage, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(CustomEncoderWithInjectionMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.CustomEncoderWithInjectionMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage,
                await new TestMessageEncoderWithInjection(services.GetRequiredService<IInjectableService>()).DecodeAsync(new MemoryStream(messages[0].Data.ToArray()))
            );
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithMessageWithDefinedEncryptor()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new CustomEncryptorMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<CustomEncryptorMessage>(testMessage, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(CustomEncryptorMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.AreEqual(Constants.CustomEncryptorMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            var decodedData = await new TestMessageEncryptor().DecryptAsync(new MemoryStream(messages[0].Data.ToArray()), messages[0].Header);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<CustomEncryptorMessage>(decodedData, cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithMessageWithDefinedServiceInjectableEncryptor()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new CustomEncryptorWithInjectionMessage("testMessage");
            var serviceName = "TestPublishAsyncWithMessageWithDefinedServiceInjectableEncryptor";
            var services = Helper.ProduceServiceProvider(serviceName);

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, serviceProvider: services);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<CustomEncryptorWithInjectionMessage>(testMessage, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(CustomEncryptorWithInjectionMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.AreEqual(Constants.CustomEncryptorWithInjectionMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            var decodedData = await new TestMessageEncryptorWithInjection(services.GetRequiredService<IInjectableService>()).DecryptAsync(new MemoryStream(messages[0].Data.ToArray()), messages[0].Header);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<CustomEncryptorWithInjectionMessage>(decodedData, cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithNoMessageChannelThrowsError()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new NoChannelMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var exception = await Assert.ThrowsExactlyAsync<MessageChannelNullException>(async () => await contractConnection.PublishAsync<NoChannelMessage>(testMessage, cancellationToken: TestContext.CancellationToken));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual("message must have a channel value (Parameter 'channel')", exception.Message);
            Assert.AreEqual("channel", exception.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithToLargeAMessageThrowsError()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            serviceConnection.Setup(x => x.MaxMessageBodySize)
                .Returns(1);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var exception = await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(async () => await contractConnection.PublishAsync<BasicMessage>(testMessage, cancellationToken: TestContext.CancellationToken));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            Assert.StartsWith($"message data exceeds maxmium message size (MaxSize:{serviceConnection.Object.MaxMessageBodySize},", exception.Message);
            Assert.AreEqual("message", exception.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithTwoDifferentMessageTypes()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage1 = new BasicMessage("testMessage1");
            var testMessage2 = new NoChannelMessage("testMessage2");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.PublishAsync<BasicMessage>(testMessage1, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Start();
            var result2 = await contractConnection.PublishAsync<NoChannelMessage>(testMessage2, channel: "TestChannel2", cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.AreEqual(transmissionResult, result1);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage1, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: TestContext.CancellationToken));

            Assert.IsNotNull(result2);
            Assert.AreEqual(transmissionResult, result2);
            Assert.AreEqual("TestChannel2", messages[1].Channel);
            Assert.AreEqual(0, messages[1].Header.Keys.Count());
            Assert.AreEqual(Constants.NoChannelMessageType, messages[1].MessageTypeID);
            Assert.IsGreaterThan(0, messages[1].Data.Length);
            Assert.AreEqual(testMessage2, await JsonSerializer.DeserializeAsync<NoChannelMessage>(new MemoryStream(messages[1].Data.ToArray()), cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithToManyConnectionMatches()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MappedServiceInstance()
                .RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object)
                .RegisterServiceConnection((props) => Equals(props.messageType, typeof(BasicMessage)), $"{ServiceName}2", serviceConnection.Object);
            #endregion

            #region Act
            var error = await Assert.ThrowsExactlyAsync<TooManyConnectionMatchesException>(async () => await contractConnection.PublishAsync<BasicMessage>(testMessage, cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithNoConnectionMatch()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MappedServiceInstance()
                .RegisterServiceConnection((props) => false, ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var error = await Assert.ThrowsExactlyAsync<NoConnectionMatchException>(async () => await contractConnection.PublishAsync<BasicMessage>(testMessage, cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task TestPublishAsyncWithTelemetryData(bool withLinking)
        {
            #region Arrange
            (var _, var capturedActivities, var sourceName) = ConnectionHelper.SetupTelemetry();
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MappedServiceInstance()
                .RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object)
                .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: withLinking);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.AreEqual((withLinking ? 2 : 0), messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: TestContext.CancellationToken));
            Assert.HasCount(1, capturedActivities);
            ConnectionHelper.ValidatePublishActivity<BasicMessage>(
                messages[0],
                capturedActivities[0],
                "MQContract.PublishMessage",
                serviceConnection.Object.GetType(),
                true,
                withLinking,
                connectionName: ServiceName
            );
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicMessage), false)]
        public async Task TestPublishAsyncAndRetryFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString(), Error: new(new Exception("error occured"), false));
            var retryCount = 2;

            var testMessage = new BasicMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, null);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, retryCount+1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsError);
            Assert.IsNotNull(result.Error);
            Assert.IsInstanceOfType<ResilienceException>(result.Error.Exception);
            Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)result.Error.Exception).Type);
            Assert.IsNotNull(result.Error.Exception.InnerException);
            Assert.AreEqual(transmissionResult.Error!.Exception, result.Error.Exception.InnerException);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicMessage), false)]
        public async Task TestPublishAsyncAndCircuitBreakFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString(), Error: new(new Exception("error occured"), false));
            var circuitBreakCount = 1;

            var testMessage = new BasicMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, null, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            _ = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken);
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
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
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(circuitBreakCount));
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicMessage), false)]
        public async Task TestPublishAsyncWithRetryAndCircuitBreakWithFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var error = new Exception("error occured");
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, false));
            var circuitBreakCount = 2;
            var retryCount = 1;

            var testMessage = new BasicMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            IEnumerable<TransmissionResult> result = [
                await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken),
                await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken)
            ];
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, retryCount+1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            var retryFailure = result.First();
            Assert.IsTrue(retryFailure.IsError);
            Assert.IsNotNull(retryFailure.Error);
            Assert.IsInstanceOfType<ResilienceException>(retryFailure.Error.Exception);
            Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)retryFailure.Error.Exception).Type);
            Assert.IsNotNull(retryFailure.Error.Exception.InnerException);
            Assert.AreEqual(transmissionResult.Error!.Exception, retryFailure.Error.Exception.InnerException);
            var circuitBreakFailure = result.Last();
            Assert.IsTrue(circuitBreakFailure.IsError);
            Assert.IsNotNull(circuitBreakFailure.Error);
            Assert.IsInstanceOfType<ResilienceException>(circuitBreakFailure.Error.Exception);
            Assert.AreEqual(ResilienceTypes.CircuitBreak, ((ResilienceException)circuitBreakFailure.Error.Exception).Type);
            Assert.IsNotNull(circuitBreakFailure.Error.Exception.InnerException);
            Assert.IsInstanceOfType<BrokenCircuitException>(circuitBreakFailure.Error.Exception.InnerException);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicMessage), false)]
        public async Task TestPublishAsyncWithRetryAndCircuitBreakWithoutTripping(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var error = new Exception("error occured");
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, true));
            var circuitBreakCount = 2;
            var retryCount = 1;

            var testMessage = new BasicMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            IEnumerable<TransmissionResult> result = [
                await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken),
                await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken),
                await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken)
            ];
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, result.Count(), TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsTrue(Array.TrueForAll(result.ToArray(), r => r.IsError
            && r.Error!=null
            && r.Error.Exception is Exception ex
            && Equals(error, ex)));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(result.Count()));
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithRetryAndCircuitBreakFailureAndEnsuringPriority()
        {
            #region Arrange
            var channel = "testChannel";
            var messageType = typeof(BasicMessage);
            var error = new Exception("Failed");
            var circuitBreakCount = 2;
            var retryCount = 1;
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, false));

            var testMessage = new BasicMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, null, false, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, null, messageType, false, retryCount+1, circuitBreakCount+1);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var channelRetryResult = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken);
            var channelCircuitBreakResult = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel, cancellationToken: TestContext.CancellationToken);
            var typeRetryResult = await contractConnection.PublishAsync<BasicMessage>(testMessage, cancellationToken: TestContext.CancellationToken);
            var typeCircuitBreakResult = await contractConnection.PublishAsync<BasicMessage>(testMessage, cancellationToken: TestContext.CancellationToken);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, (circuitBreakCount*2)+1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(channelRetryResult);
            Assert.IsNotNull(channelCircuitBreakResult);
            Assert.IsNotNull(typeRetryResult);
            Assert.IsNotNull(typeCircuitBreakResult);

            Assert.IsTrue(Array.TrueForAll([channelRetryResult, typeRetryResult], (res) => res.IsError
            && res.Error!=null
            && res.Error.Exception is ResilienceException re
            && Equals(ResilienceTypes.Retry, re.Type)
            && Equals(error, re.InnerException)));

            Assert.IsTrue(Array.TrueForAll([channelCircuitBreakResult, typeCircuitBreakResult], (res) => res.IsError
            && res.Error!=null
            && res.Error.Exception is ResilienceException re
            && Equals(ResilienceTypes.CircuitBreak, re.Type)
            && re.InnerException is BrokenCircuitException));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(((retryCount+1)*2)+1));
            #endregion
        }

        public TestContext TestContext { get; set; }
    }
}
