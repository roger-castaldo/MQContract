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

namespace AutomatedTesting.ConnectionTests.MultiService
{
    [TestClass]
    public class PublishTests
    {
        private const string ServiceName = "testService";
        private const string ServiceName2 = "testService2";

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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
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
            Assert.HasCount(1, messages);
            Assert.AreEqual(result.ID, messages[0].ID);
            Assert.AreEqual(1, result.Results.Count());
            Assert.AreEqual(ServiceName, result.Results.First().ServiceName);
            Assert.IsFalse(result.Results.First().IsError);
            Assert.IsFalse(result.HasError);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: $"Not{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}");
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.HasCount(1, messages);
            Assert.AreEqual(result.ID, messages[0].ID);
            Assert.AreEqual(1, result.Results.Count());
            Assert.AreEqual(ServiceName, result.Results.First().ServiceName);
            Assert.IsFalse(result.Results.First().IsError);
            Assert.IsFalse(result.HasError);
            Assert.AreEqual($"Not{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}", messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, JsonSerializer.Deserialize<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage, messageHeader: messageHeader);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.HasCount(1, messages);
            Assert.AreEqual(result.ID, messages[0].ID);
            Assert.AreEqual(1, result.Results.Count());
            Assert.AreEqual(ServiceName, result.Results.First().ServiceName);
            Assert.IsFalse(result.Results.First().IsError);
            Assert.IsFalse(result.HasError);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(Constants.BasicMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, JsonSerializer.Deserialize<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
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
            Assert.HasCount(1, messages);
            Assert.AreEqual(result.ID, messages[0].ID);
            Assert.AreEqual(1, result.Results.Count());
            Assert.AreEqual(ServiceName, result.Results.First().ServiceName);
            Assert.IsFalse(result.Results.First().IsError);
            Assert.IsFalse(result.HasError);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, messages[0].Header.Keys.Count());
            Assert.AreEqual("true", messages[0].Header[messages[0].Header.Keys.First()]);
            Assert.AreEqual(Constants.BasicMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(
                new GZipStream(new MemoryStream(messages[0].Data.ToArray()), CompressionMode.Decompress)
            ));
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

            var contractConnection = ContractConnection.MultiServiceInstance(defaultMessageEncoder: globalEncoder.Object)
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
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
            Assert.HasCount(1, messages);
            Assert.AreEqual(result.ID, messages[0].ID);
            Assert.AreEqual(1, result.Results.Count());
            Assert.AreEqual(ServiceName, result.Results.First().ServiceName);
            Assert.IsFalse(result.Results.First().IsError);
            Assert.IsFalse(result.HasError);
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
                .ReturnsAsync((byte[] binary) => (binary.Reverse().ToArray(), headers));

            var contractConnection = ContractConnection.MultiServiceInstance(defaultMessageEncryptor: globalEncryptor.Object)
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
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
            Assert.HasCount(1, messages);
            Assert.AreEqual(result.ID, messages[0].ID);
            Assert.AreEqual(1, result.Results.Count());
            Assert.AreEqual(ServiceName, result.Results.First().ServiceName);
            Assert.IsFalse(result.Results.First().IsError);
            Assert.IsFalse(result.HasError);
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<NamedAndVersionedMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.HasCount(1, messages);
            Assert.AreEqual(result.ID, messages[0].ID);
            Assert.AreEqual(1, result.Results.Count());
            Assert.AreEqual(ServiceName, result.Results.First().ServiceName);
            Assert.IsFalse(result.Results.First().IsError);
            Assert.IsFalse(result.HasError);
            Assert.AreEqual(typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.NamedAndVersionedMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, JsonSerializer.Deserialize<NamedAndVersionedMessage>(new MemoryStream(messages[0].Data.ToArray())));
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<CustomEncoderMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.HasCount(1, messages);
            Assert.AreEqual(result.ID, messages[0].ID);
            Assert.AreEqual(1, result.Results.Count());
            Assert.AreEqual(ServiceName, result.Results.First().ServiceName);
            Assert.IsFalse(result.Results.First().IsError);
            Assert.IsFalse(result.HasError);
            Assert.AreEqual(typeof(CustomEncoderMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
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

            var contractConnection = ContractConnection.MultiServiceInstance(serviceProvider: services)
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<CustomEncoderWithInjectionMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.HasCount(1, messages);
            Assert.AreEqual(result.ID, messages[0].ID);
            Assert.AreEqual(1, result.Results.Count());
            Assert.AreEqual(ServiceName, result.Results.First().ServiceName);
            Assert.IsFalse(result.Results.First().IsError);
            Assert.IsFalse(result.HasError);
            Assert.AreEqual(typeof(CustomEncoderWithInjectionMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<CustomEncryptorMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.HasCount(1, messages);
            Assert.AreEqual(result.ID, messages[0].ID);
            Assert.AreEqual(1, result.Results.Count());
            Assert.AreEqual(ServiceName, result.Results.First().ServiceName);
            Assert.IsFalse(result.Results.First().IsError);
            Assert.AreEqual(typeof(CustomEncryptorMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(Constants.CustomEncryptorMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            var decodedData = await new TestMessageEncryptor().DecryptAsync(new MemoryStream(messages[0].Data.ToArray()), messages[0].Header);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<CustomEncryptorMessage>(decodedData));
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

            var contractConnection = ContractConnection.MultiServiceInstance(serviceProvider: services)
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<CustomEncryptorWithInjectionMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.HasCount(1, messages);
            Assert.AreEqual(result.ID, messages[0].ID);
            Assert.AreEqual(1, result.Results.Count());
            Assert.AreEqual(ServiceName, result.Results.First().ServiceName);
            Assert.IsFalse(result.Results.First().IsError);
            Assert.AreEqual(typeof(CustomEncryptorWithInjectionMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(Constants.CustomEncryptorWithInjectionMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            var decodedData = await new TestMessageEncryptorWithInjection(services.GetRequiredService<IInjectableService>()).DecryptAsync(new MemoryStream(messages[0].Data.ToArray()), messages[0].Header);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<CustomEncryptorWithInjectionMessage>(decodedData));
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var exception = await Assert.ThrowsExactlyAsync<MessageChannelNullException>(async () => await contractConnection.PublishAsync<NoChannelMessage>(testMessage));
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var exception = await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(async () => await contractConnection.PublishAsync<BasicMessage>(testMessage));
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

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.PublishAsync<BasicMessage>(testMessage1);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Start();
            var result2 = await contractConnection.PublishAsync<NoChannelMessage>(testMessage2, channel: "TestChannel2");
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.ID, messages[0].ID);
            Assert.AreEqual(1, result1.Results.Count());
            Assert.AreEqual(ServiceName, result1.Results.First().ServiceName);
            Assert.IsFalse(result1.Results.First().IsError);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage1, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));

            Assert.IsNotNull(result2);
            Assert.AreEqual(result2.ID, messages[1].ID);
            Assert.AreEqual(1, result2.Results.Count());
            Assert.AreEqual(ServiceName, result2.Results.First().ServiceName);
            Assert.IsFalse(result2.Results.First().IsError);
            Assert.AreEqual("TestChannel2", messages[1].Channel);
            Assert.AreEqual(0, messages[1].Header.Keys.Count());
            Assert.AreEqual(Constants.NoChannelMessageType, messages[1].MessageTypeID);
            Assert.IsGreaterThan(0, messages[1].Data.Length);
            Assert.AreEqual(testMessage2, await JsonSerializer.DeserializeAsync<NoChannelMessage>(new MemoryStream(messages[1].Data.ToArray())));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task TestPublishAsyncWithTelemetryData(bool withLinking)
        {
            #region Arrange
            (var listener, var capturedActivities, var sourceName) = ConnectionHelper.SetupTelemetry();
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");

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
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.HasCount(1, messages);
            Assert.AreEqual(result.ID, messages[0].ID);
            Assert.AreEqual(1, result.Results.Count());
            Assert.AreEqual(ServiceName, result.Results.First().ServiceName);
            Assert.IsFalse(result.Results.First().IsError);
            Assert.IsFalse(result.HasError);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual((withLinking ? 2 : 0), messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray())));
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

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            var serviceConnection2 = new Mock<IMessageServiceConnection>();
            serviceConnection2.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransmissionResult(Guid.NewGuid().ToString()));

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, null);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, retryCount, null);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var results = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(results);
            var result = results.Results.First(r => Equals(ServiceName, r.ServiceName));
            Assert.IsTrue(result.IsError);
            Assert.IsNotNull(result.Error);
            Assert.IsInstanceOfType<ResilienceException>(result.Error.Exception);
            Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)result.Error.Exception).Type);
            Assert.IsNotNull(result.Error.Exception.InnerException);
            Assert.AreEqual(transmissionResult.Error!.Exception, result.Error.Exception.InnerException);
            Assert.IsTrue(results.Results.Where(r => Equals(ServiceName2, r.ServiceName)).All(r => !r.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection2.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
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

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            var serviceConnection2 = new Mock<IMessageServiceConnection>();
            serviceConnection2.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransmissionResult(Guid.NewGuid().ToString()));

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, null, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, null, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            _ = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel);
            var results = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(results);
            var result = results.Results.First(r => Equals(ServiceName, r.ServiceName));
            Assert.IsTrue(result.IsError);
            Assert.IsNotNull(result.Error);
            Assert.IsInstanceOfType<ResilienceException>(result.Error.Exception);
            Assert.AreEqual(ResilienceTypes.CircuitBreak, ((ResilienceException)result.Error.Exception).Type);
            Assert.IsNotNull(result.Error.Exception.InnerException);
            Assert.IsInstanceOfType<BrokenCircuitException>(result.Error.Exception.InnerException);
            Assert.IsTrue(results.Results.Where(r => Equals(ServiceName2, r.ServiceName)).All(r => !r.IsError));
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
        public async Task TestPublishAsyncWithRetryAndCircuitBreakWithFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var error = new Exception("error occured");
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, false));
            var circuitBreakCount = 2;
            var retryCount = 1;

            var testMessage = new BasicMessage("testMessage");

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            var serviceConnection2 = new Mock<IMessageServiceConnection>();
            serviceConnection2.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransmissionResult(Guid.NewGuid().ToString()));

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var retryResults = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel);
            var circuitBreakResults = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(retryResults);
            Assert.IsNotNull(circuitBreakResults);
            var retryFailure = retryResults.Results.First(r => Equals(ServiceName, r.ServiceName));
            Assert.IsTrue(retryFailure.IsError);
            Assert.IsNotNull(retryFailure.Error);
            Assert.IsInstanceOfType<ResilienceException>(retryFailure.Error.Exception);
            Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)retryFailure.Error.Exception).Type);
            Assert.AreEqual(error, retryFailure.Error.Exception.InnerException);
            Assert.IsTrue(Array.TrueForAll(circuitBreakResults.Results.Where(r => Equals(ServiceName, r.ServiceName)).ToArray(), result => result.IsError
            && result.Error!=null
            && result.Error.Exception is ResilienceException re
            && Equals(ResilienceTypes.CircuitBreak, re.Type)
            && re.InnerException is BrokenCircuitException));
            Assert.IsTrue(Array.TrueForAll(retryResults.Results.Where(r => Equals(r.ServiceName, ServiceName2)).ToArray(), r => !r.IsError));
            Assert.IsTrue(Array.TrueForAll(circuitBreakResults.Results.Where(r => Equals(r.ServiceName, ServiceName2)).ToArray(), r => !r.IsError));
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
        public async Task TestPublishAsyncWithRetryAndCircuitBreakWithoutTripping(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var error = new Exception("error occured");
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, true));
            var circuitBreakCount = 2;
            var retryCount = 1;

            var testMessage = new BasicMessage("testMessage");

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            var serviceConnection2 = new Mock<IMessageServiceConnection>();
            serviceConnection2.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransmissionResult(Guid.NewGuid().ToString()));

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object)
                .RegisterServiceConnection(ServiceName2, serviceConnection2.Object);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName2, channel, messageType, useGenerics, retryCount, circuitBreakCount);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var retryResults = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel);
            var circuitBreakResults = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(retryResults);
            Assert.IsNotNull(circuitBreakResults);
            Assert.IsTrue(Array.TrueForAll(retryResults.Results.Where(r => Equals(r.ServiceName, ServiceName)).ToArray(), r => r.IsError && Equals(error, r.Error!.Exception) && r.Error!.IsFatal));
            Assert.IsTrue(Array.TrueForAll(circuitBreakResults.Results.Where(r => Equals(r.ServiceName, ServiceName)).ToArray(), r => r.IsError && Equals(error, r.Error!.Exception) && r.Error!.IsFatal));
            Assert.IsTrue(Array.TrueForAll(retryResults.Results.Where(r => Equals(r.ServiceName, ServiceName2)).ToArray(), r => !r.IsError));
            Assert.IsTrue(Array.TrueForAll(circuitBreakResults.Results.Where(r => Equals(r.ServiceName, ServiceName2)).ToArray(), r => !r.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection2.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncWithRetryAndCircuitBreakFailureAndEnsuringPriority()
        {
            #region Arrange
            var serviceName3 = "testService3";
            var channel = "testChannel";
            var messageType = typeof(BasicMessage);
            var error = new Exception("Failed");
            var circuitBreakCount = 2;
            var retryCount = 1;
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString(), Error: new(error, false));

            var testMessage = new BasicMessage("testMessage");

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            var serviceConnection2 = new Mock<IMessageServiceConnection>();
            serviceConnection2.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransmissionResult(Guid.NewGuid().ToString()));
            var serviceConnection3 = new Mock<IMessageServiceConnection>();
            serviceConnection3.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

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
            var channelRetryResults = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel);
            var channelCircuitBreakResults = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel);
            var typeRetryResults = await contractConnection.PublishAsync<BasicMessage>(testMessage);
            var typeCircuitBreakResults = await contractConnection.PublishAsync<BasicMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(channelRetryResults);
            Assert.IsNotNull(channelCircuitBreakResults);
            Assert.IsTrue(Array.TrueForAll(channelRetryResults.Results.Where(r => Equals(ServiceName, r.ServiceName) || Equals(serviceName3, r.ServiceName)).ToArray(),
                retryFailure => retryFailure.IsError
                && retryFailure.Error!=null
                && retryFailure.Error.Exception is ResilienceException resilienceException
                && Equals(ResilienceTypes.Retry, resilienceException.Type)
                && Equals(error, resilienceException.InnerException)
            ));
            Assert.IsTrue(Array.TrueForAll(channelCircuitBreakResults.Results.Where(r => Equals(ServiceName, r.ServiceName)).ToArray(), result => result.IsError
            && result.Error!=null
            && result.Error.Exception is ResilienceException re
            && Equals(ResilienceTypes.CircuitBreak, re.Type)
            && re.InnerException is BrokenCircuitException));
            Assert.IsNotNull(typeRetryResults);
            Assert.IsNotNull(typeCircuitBreakResults);
            Assert.IsTrue(Array.TrueForAll(typeRetryResults.Results.Where(r => Equals(ServiceName, r.ServiceName) || Equals(serviceName3, r.ServiceName)).ToArray(),
                retryFailure => retryFailure.IsError
                && retryFailure.Error!=null
                && retryFailure.Error.Exception is ResilienceException resilienceException
                && Equals(ResilienceTypes.Retry, resilienceException.Type)
                && Equals(error, resilienceException.InnerException)
            ));
            Assert.IsTrue(Array.TrueForAll(typeCircuitBreakResults.Results.Where(r => Equals(ServiceName, r.ServiceName)).ToArray(), result => result.IsError
            && result.Error!=null
            && result.Error.Exception is ResilienceException re
            && Equals(ResilienceTypes.CircuitBreak, re.Type)
            && re.InnerException is BrokenCircuitException));

            Assert.IsTrue(Array.TrueForAll(channelRetryResults.Results.Where(r => Equals(r.ServiceName, ServiceName2)).ToArray(), r => !r.IsError));
            Assert.IsTrue(Array.TrueForAll(channelCircuitBreakResults.Results.Where(r => Equals(r.ServiceName, ServiceName2)).ToArray(), r => !r.IsError));
            Assert.IsTrue(Array.TrueForAll(typeRetryResults.Results.Where(r => Equals(r.ServiceName, ServiceName2)).ToArray(), r => !r.IsError));
            Assert.IsTrue(Array.TrueForAll(typeCircuitBreakResults.Results.Where(r => Equals(r.ServiceName, ServiceName2)).ToArray(), r => !r.IsError));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(((retryCount+1)*2)+1));
            serviceConnection2.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
            serviceConnection3.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(((retryCount+1)*2)+1));
            #endregion
        }
    }
}
