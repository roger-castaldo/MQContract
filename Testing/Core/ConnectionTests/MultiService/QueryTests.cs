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
    public class QueryTests
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


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
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
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, timeouts);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.First().Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithDifferentChannelName()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: $"Not{typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel}");
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual($"Not{typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel}", messages[0].Channel);
            Assert.HasCount(1, timeouts);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.First().Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithMessageHeaders()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var messageHeader = new MessageHeader([new("testing", "testing")]);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, messageHeader: messageHeader);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, timeouts);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.First().Result);
            Assert.AreEqual(messageHeader.Keys.Count(), messages[0].Header.Keys.Count());
            Assert.IsTrue(messageHeader.Keys.All(k => messages[0].Header.Keys.Contains(k)));
            Assert.IsTrue(messageHeader.Keys.All(k => Equals(messages[0].Header[k], messageHeader[k])));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithTimeout()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);
            var timeout = TimeSpan.FromDays(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, timeout: timeout);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, timeouts);
            Assert.AreEqual(timeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.First().Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithCompressionDueToMessageSize()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("AAAAAAAAAAAAAAAAAAAaaaaaaaaaaaaaaaaaaaa");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);
            serviceConnection.Setup(x => x.MaxMessageBodySize)
                .Returns(37);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, timeouts);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(1, messages[0].Header.Keys.Count());
            Assert.AreEqual("true", messages[0].Header[messages[0].Header.Keys.First()]);
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(
                new GZipStream(new MemoryStream(messages[0].Data.ToArray()), CompressionMode.Decompress)
            ));
            Assert.AreEqual(responseMessage, result.First().Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithGlobalEncoder()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var encodedData = Encoding.ASCII.GetBytes(testMessage.TypeName);
            var responseMessage = new BasicResponseMessage("testResponse");
            var responseData = Encoding.ASCII.GetBytes(responseMessage.TestName).AsMemory();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var globalEncoder = new Mock<IMessageEncoder>();
            globalEncoder.Setup(x => x.EncodeAsync(It.IsAny<BasicQueryMessage>()))
                .ReturnsAsync(encodedData);
            globalEncoder.Setup(x => x.DecodeAsync<BasicResponseMessage>(It.IsAny<Stream>()))
                .ReturnsAsync((Stream str) =>
                {
                    var reader = new StreamReader(str);
                    var result = new BasicResponseMessage(reader.ReadToEnd());
                    return result;
                });

            var contractConnection = ContractConnection.MultiServiceInstance(defaultMessageEncoder: globalEncoder.Object)
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, timeouts);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(Convert.ToBase64String(encodedData), Convert.ToBase64String(messages[0].Data.ToArray()));
            Assert.AreEqual(responseMessage, result.First().Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithGlobalEncryptor()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray().Reverse().ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];
            List<byte[]> binaries = [];
            Dictionary<string, string?> headers = new([
                    new KeyValuePair<string,string?>("test","test")
                ]);

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var globalEncryptor = new Mock<IMessageEncryptor>();
            globalEncryptor.Setup(x => x.EncryptAsync(Capture.In(binaries)))
                .ReturnsAsync((byte[] binary) => (binary.Reverse().ToArray(), headers));
            globalEncryptor.Setup(x => x.DecryptAsync(It.IsAny<Stream>(), It.IsAny<MessageHeader>()))
                .ReturnsAsync((Stream source, MessageHeader headers) =>
                {
                    var buff = new byte[source.Length];
                    _ = source.Read(buff, 0, buff.Length);
                    return new MemoryStream(buff.Reverse().ToArray());
                });

            var contractConnection = ContractConnection.MultiServiceInstance(defaultMessageEncryptor: globalEncryptor.Object)
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, timeouts);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray().Reverse().ToArray())));
            Assert.AreEqual(responseMessage, result.First().Result);
            Assert.AreEqual(headers.Count, messages[0].Header.Keys.Count());
            Assert.IsTrue(headers.Keys.All(k => messages[0].Header.Keys.Contains(k)));
            Assert.AreEqual(headers[headers.Keys.First()], messages[0].Header[headers.Keys.First()]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithTimeoutAttribute()
        {
            #region Arrange
            var testMessage = new TimeoutMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<TimeoutMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(TimeoutMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, timeouts);
            Assert.AreEqual(TimeSpan.FromMilliseconds(typeof(TimeoutMessage).GetCustomAttribute<QueryMessageAttribute>(false)?.ResponseTimeout.TotalMilliseconds ?? 0), timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.TimeoutMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<TimeoutMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.First().Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithNamedAndVersionedMessage()
        {
            #region Arrange
            var testMessage = new NamedAndVersionedMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<NamedAndVersionedMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, timeouts);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.NamedAndVersionedMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<NamedAndVersionedMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.First().Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithMessageWithDefinedEncoder()
        {
            #region Arrange
            var testMessage = new CustomEncoderMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<CustomEncoderMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(CustomEncoderMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, timeouts);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.CustomEncoderMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await new TestMessageEncoder().DecodeAsync(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.First().Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithMessageWithDefinedServiceInjectableEncoder()
        {
            #region Arrange
            var testMessage = new CustomEncoderWithInjectionMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);
            var serviceName = "TestPublishAsyncWithMessageWithDefinedServiceInjectableEncoder";
            var services = Helper.ProduceServiceProvider(serviceName);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance(serviceProvider: services)
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<CustomEncoderWithInjectionMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(CustomEncoderWithInjectionMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, timeouts);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.CustomEncoderWithInjectionMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage,
                await new TestMessageEncoderWithInjection(services.GetRequiredService<IInjectableService>()).DecodeAsync(new MemoryStream(messages[0].Data.ToArray()))
            );
            Assert.AreEqual(responseMessage, result.First().Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithMessageWithDefinedEncryptor()
        {
            #region Arrange
            var testMessage = new CustomEncryptorMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<CustomEncryptorMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(CustomEncryptorMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, timeouts);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(Constants.CustomEncryptorMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            var decodedData = await new TestMessageEncryptor().DecryptAsync(new MemoryStream(messages[0].Data.ToArray()), messages[0].Header);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<CustomEncryptorMessage>(decodedData));
            Assert.AreEqual(responseMessage, result.First().Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithMessageWithDefinedServiceInjectableEncryptor()
        {
            #region Arrange
            var testMessage = new CustomEncryptorWithInjectionMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);
            var serviceName = "TestPublishAsyncWithMessageWithDefinedServiceInjectableEncryptor";
            var services = Helper.ProduceServiceProvider(serviceName);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance(serviceProvider: services)
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<CustomEncryptorWithInjectionMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(CustomEncryptorWithInjectionMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, timeouts);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(Constants.CustomEncryptorWithInjectionMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            var decodedData = await new TestMessageEncryptorWithInjection(services.GetRequiredService<IInjectableService>()).DecryptAsync(new MemoryStream(messages[0].Data.ToArray()), messages[0].Header);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<CustomEncryptorWithInjectionMessage>(decodedData));
            Assert.AreEqual(responseMessage, result.First().Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithNoMessageChannelThrowsError()
        {
            #region Arrange
            var testMessage = new NoChannelMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var exception = await Assert.ThrowsExactlyAsync<MessageChannelNullException>(async () => await contractConnection.QueryAsync<NoChannelMessage, BasicResponseMessage>(testMessage));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual("message must have a channel value (Parameter 'channel')", exception.Message);
            Assert.AreEqual("channel", exception.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithToLargeAMessageThrowsError()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);
            serviceConnection.Setup(x => x.MaxMessageBodySize)
                .Returns(1);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var exception = await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            Assert.StartsWith($"message data exceeds maxmium message size (MaxSize:{serviceConnection.Object.MaxMessageBodySize},", exception.Message);
            Assert.AreEqual("message", exception.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithTwoDifferentMessageTypes()
        {
            #region Arrange
            var testMessage1 = new BasicQueryMessage("testMessage");
            var testMessage2 = new NoChannelMessage("testMessage2");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage1);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Start();
            var result2 = await contractConnection.QueryAsync<NoChannelMessage, BasicResponseMessage>(testMessage2, channel: "TestChannel2");
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.AreEqual(1, result1.Count());
            Assert.AreEqual(queryResult.ID, result1.First().ID);
            Assert.IsNull(result1.First().Error);
            Assert.IsFalse(result1.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(2, timeouts);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage1, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result1.First().Result);

            Assert.IsNotNull(result2);
            Assert.AreEqual(1, result2.Count());
            Assert.AreEqual(queryResult.ID, result2.First().ID);
            Assert.IsNull(result2.First().Error);
            Assert.IsFalse(result2.First().IsError);
            Assert.AreEqual("TestChannel2", messages[1].Channel);
            Assert.AreEqual(defaultTimeout, timeouts[1]);
            Assert.AreEqual(0, messages[1].Header.Keys.Count());
            Assert.AreEqual(Constants.NoChannelMessageType, messages[1].MessageTypeID);
            Assert.IsGreaterThan(0, messages[1].Data.Length);
            Assert.AreEqual(testMessage2, await JsonSerializer.DeserializeAsync<NoChannelMessage>(new MemoryStream(messages[1].Data.ToArray())));
            Assert.AreEqual(responseMessage, result2.First().Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithAttributeReturnType()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage>(testMessage);
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, timeouts);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual(Constants.BasicQueryMessageType, messages[0].MessageTypeID);
            Assert.IsGreaterThan(0, messages[0].Data.Length);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.First().Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithAttributeReturnTypeThrowingTimeoutException()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");

            var defaultTimeout = TimeSpan.FromMinutes(1);

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Throws<TimeoutException>();
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var exception = await Assert.ThrowsExactlyAsync<QueryTimeoutException>(async () => await contractConnection.QueryAsync<BasicQueryMessage>(testMessage));
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithNoReturnType()
        {
            #region Arrange
            var testMessage = new NoChannelMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.MultiServiceInstance()
                .RegisterServiceConnection(ServiceName, serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var exception = await Assert.ThrowsExactlyAsync<UnknownResponseTypeException>(async () => await contractConnection.QueryAsync<NoChannelMessage>(testMessage));
            stopwatch.Stop();
            Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual($"The attempt to call a query response with the incoming message of type {typeof(NoChannelMessage).FullName} does not have a determined response type. (Parameter 'ResponseType')", exception.Message);
            Assert.AreEqual("ResponseType", exception.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
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


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
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
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(messages.Count, result.Count());
            Assert.AreEqual(queryResult.ID, result.First().ID);
            Assert.IsNull(result.First().Error);
            Assert.IsFalse(result.First().IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, messages[0].Channel);
            Assert.HasCount(1, timeouts);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
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
                includePublish: false,
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
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
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
            var transmitionError = new TransmissionException(error, false);
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(transmitionError);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);
            var serviceConnection2 = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection2.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
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
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection2.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
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
            var transmitionError = new TransmissionException(error, false);
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(transmitionError);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);
            var serviceConnection2 = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection2.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
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
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(circuitBreakCount));
            serviceConnection2.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicQueryMessage), false)]
        public async Task TestQueryAsyncAndCircuitBreakWithFailure(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var error = new Exception("test error");
            var circuitBreakCount = 2;
            var retryCount = 1;
            var transmitionError = new TransmissionException(error, false);
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(transmitionError);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);
            var serviceConnection2 = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection2.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
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
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection2.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        [DataRow(null, null, false)]
        [DataRow(null, null, true)]
        [DataRow("testChannel", null, false)]
        [DataRow(null, typeof(BasicQueryMessage), false)]
        public async Task TestQueryAsyncAndCircuitBreakWithoutTripping(string? channel, Type? messageType, bool useGenerics)
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var error = new Exception("test error");
            var circuitBreakCount = 2;
            var retryCount = 1;
            var transmitionError = new TransmissionException(error, true);
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(transmitionError);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);
            var serviceConnection2 = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection2.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
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
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection2.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
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
            var transmitionError = new TransmissionException(error, false);
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(transmitionError);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);
            var serviceConnection2 = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection2.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection2.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);
            var serviceConnection3 = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection3.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(transmitionError);
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
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            serviceConnection2.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection3.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount+1));
            #endregion
        }
    }
}
