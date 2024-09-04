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
    public class QueryTests
    {
        [TestMethod]
        public async Task TestQueryAsyncWithNoExtendedAspects()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithDifferentChannelName()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: $"Not{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}");
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual($"Not{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}", messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithMessageHeaders()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var messageHeader = new MessageHeader([new("testing", "testing")]);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, messageHeader: messageHeader);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual("U-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            Assert.AreEqual(messageHeader.Keys.Count(), messages[0].Header.Keys.Count());
            Assert.IsTrue(messageHeader.Keys.All(k => messages[0].Header.Keys.Contains(k)));
            Assert.IsTrue(messageHeader.Keys.All(k => Equals(messages[0].Header[k], messageHeader[k])));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithTimeout()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);
            var timeout = TimeSpan.FromDays(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, timeout: timeout);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(timeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithCompressionDueToMessageSize()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("AAAAAAAAAAAAAAAAAAAaaaaaaaaaaaaaaaaaaaa");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);
            serviceConnection.Setup(x => x.MaxMessageBodySize)
                .Returns(37);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("C-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(
                new GZipStream(new MemoryStream(messages[0].Data.ToArray()), CompressionMode.Decompress)
            ));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithGlobalEncoder()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var encodedData = ASCIIEncoding.ASCII.GetBytes(testMessage.TypeName);
            var responseMessage = new BasicResponseMessage("testResponse");
            var responseData = ASCIIEncoding.ASCII.GetBytes(responseMessage.TestName).AsMemory();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var globalEncoder = new Mock<IMessageEncoder>();
            globalEncoder.Setup(x => x.EncodeAsync<BasicQueryMessage>(It.IsAny<BasicQueryMessage>()))
                .ReturnsAsync(encodedData);
            globalEncoder.Setup(x => x.DecodeAsync<BasicResponseMessage>(It.IsAny<Stream>()))
                .ReturnsAsync((Stream str) =>
                {
                    var reader = new StreamReader(str);
                    var result = new BasicResponseMessage(reader.ReadToEnd());
                    return result;
                });

            var contractConnection = new ContractConnection(serviceConnection.Object, defaultMessageEncoder: globalEncoder.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(Convert.ToBase64String(encodedData), Convert.ToBase64String(messages[0].Data.ToArray()));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithGlobalEncryptor()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray().Reverse().ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];
            List<byte[]> binaries = [];
            Dictionary<string, string?> headers = new([
                    new KeyValuePair<string,string?>("test","test")
                ]);

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var globalEncryptor = new Mock<IMessageEncryptor>();
            globalEncryptor.Setup(x => x.EncryptAsync(Capture.In<byte[]>(binaries), out headers))
                .ReturnsAsync((byte[] binary, Dictionary<string, string?> h) => binary.Reverse().ToArray());
            globalEncryptor.Setup(x => x.DecryptAsync(It.IsAny<Stream>(), It.IsAny<MessageHeader>()))
                .ReturnsAsync((Stream source, MessageHeader headers) =>
                {
                    var buff = new byte[source.Length];
                    source.Read(buff, 0, buff.Length);
                    return new MemoryStream(buff.Reverse().ToArray());
                });

            var contractConnection = new ContractConnection(serviceConnection.Object, defaultMessageEncryptor: globalEncryptor.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual("U-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray().Reverse().ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            Assert.AreEqual(headers.Count, messages[0].Header.Keys.Count());
            Assert.IsTrue(headers.Keys.All(k => messages[0].Header.Keys.Contains(k)));
            Assert.AreEqual(headers[headers.Keys.First()], messages[0].Header[headers.Keys.First()]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithTimeoutAttribute()
        {
            #region Arrange
            var testMessage = new TimeoutMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<TimeoutMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsFalse(result.IsError);
            Assert.IsNull(result.Error);
            Assert.AreEqual(typeof(TimeoutMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(TimeSpan.FromMilliseconds(typeof(TimeoutMessage).GetCustomAttribute<MessageResponseTimeoutAttribute>(false)?.Value??0), timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-TimeoutMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<TimeoutMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithServiceChannelOptions()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];
            List<IServiceChannelOptions> options = [];
            var serviceChannelOptions = new TestServiceChannelOptions("QWueryAsync");

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), Capture.In<IServiceChannelOptions>(options), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, options: serviceChannelOptions);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            Assert.AreEqual(1, options.Count);
            Assert.IsInstanceOfType<TestServiceChannelOptions>(options[0]);
            Assert.AreEqual(serviceChannelOptions, options[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithNamedAndVersionedMessage()
        {
            #region Arrange
            var testMessage = new NamedAndVersionedMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<NamedAndVersionedMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual($"U-{typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageNameAttribute>(false)?.Value}-{typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageVersionAttribute>(false)?.Version}", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<NamedAndVersionedMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithMessageWithDefinedEncoder()
        {
            #region Arrange
            var testMessage = new CustomEncoderMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<CustomEncoderMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(CustomEncoderMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-CustomEncoderMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, await new TestMessageEncoder().DecodeAsync(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithMessageWithDefinedServiceInjectableEncoder()
        {
            #region Arrange
            var testMessage = new CustomEncoderWithInjectionMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);
            var serviceName = "TestPublishAsyncWithMessageWithDefinedServiceInjectableEncoder";
            var services = Helper.ProduceServiceProvider(serviceName);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object, serviceProvider: services);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<CustomEncoderWithInjectionMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(CustomEncoderWithInjectionMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-CustomEncoderWithInjectionMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage,
                await new TestMessageEncoderWithInjection(services.GetRequiredService<IInjectableService>()).DecodeAsync(new MemoryStream(messages[0].Data.ToArray()))
            );
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithMessageWithDefinedEncryptor()
        {
            #region Arrange
            var testMessage = new CustomEncryptorMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<CustomEncryptorMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(CustomEncryptorMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual("U-CustomEncryptorMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            var decodedData = await new TestMessageEncryptor().DecryptAsync(new MemoryStream(messages[0].Data.ToArray()), messages[0].Header);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<CustomEncryptorMessage>(decodedData));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithMessageWithDefinedServiceInjectableEncryptor()
        {
            #region Arrange
            var testMessage = new CustomEncryptorWithInjectionMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);
            var serviceName = "TestPublishAsyncWithMessageWithDefinedServiceInjectableEncryptor";
            var services = Helper.ProduceServiceProvider(serviceName);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object, serviceProvider: services);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<CustomEncryptorWithInjectionMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(CustomEncryptorWithInjectionMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual("U-CustomEncryptorWithInjectionMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            var decodedData = await new TestMessageEncryptorWithInjection(services.GetRequiredService<IInjectableService>()).DecryptAsync(new MemoryStream(messages[0].Data.ToArray()), messages[0].Header);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<CustomEncryptorWithInjectionMessage>(decodedData));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithNoMessageChannelThrowsError()
        {
            #region Arrange
            var testMessage = new NoChannelMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var exception = await Assert.ThrowsExceptionAsync<MessageChannelNullException>(async () => await contractConnection.QueryAsync<NoChannelMessage, BasicResponseMessage>(testMessage));
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual("message must have a channel value (Parameter 'channel')", exception.Message);
            Assert.AreEqual("channel", exception.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithToLargeAMessageThrowsError()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);
            serviceConnection.Setup(x => x.MaxMessageBodySize)
                .Returns(1);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var exception = await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage));
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.StartsWith($"message data exceeds maxmium message size (MaxSize:{serviceConnection.Object.MaxMessageBodySize},"));
            Assert.AreEqual("message", exception.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Never);
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
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage1);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Start();
            var result2 = await contractConnection.QueryAsync<NoChannelMessage, BasicResponseMessage>(testMessage2, channel: "TestChannel2");
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.AreEqual(queryResult.ID, result1.ID);
            Assert.IsNull(result1.Error);
            Assert.IsFalse(result1.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(2, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage1, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result1.Result);

            Assert.IsNotNull(result2);
            Assert.AreEqual(queryResult.ID, result2.ID);
            Assert.IsNull(result2.Error);
            Assert.IsFalse(result2.IsError);
            Assert.AreEqual("TestChannel2", messages[1].Channel);
            Assert.AreEqual(defaultTimeout, timeouts[1]);
            Assert.AreEqual(0, messages[1].Header.Keys.Count());
            Assert.AreEqual("U-NoChannelMessage-0.0.0.0", messages[1].MessageTypeID);
            Assert.IsTrue(messages[1].Data.Length>0);
            Assert.AreEqual(testMessage2, await JsonSerializer.DeserializeAsync<NoChannelMessage>(new MemoryStream(messages[1].Data.ToArray())));
            Assert.AreEqual(responseMessage, result2.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithAttributeReturnType()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, timeouts.Count);
            Assert.AreEqual(defaultTimeout, timeouts[0]);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithNoReturnType()
        {
            #region Arrange
            var testMessage = new NoChannelMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);


            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<ServiceMessage> messages = [];
            List<TimeSpan> timeouts = [];

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<TimeSpan>(timeouts), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var exception = await Assert.ThrowsExceptionAsync<UnknownResponseTypeException>(async () => await contractConnection.QueryAsync<NoChannelMessage>(testMessage));
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual($"The attempt to call a query response with the incoming message of type {typeof(NoChannelMessage).FullName} does not have a determined response type. (Parameter 'ResponseType')", exception.Message);
            Assert.AreEqual("ResponseType", exception.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }
    }
}
