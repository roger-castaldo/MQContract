using AutomatedTesting.Messages;
using Moq;
using MQContract.Attributes;
using MQContract.Interfaces.Service;
using MQContract;
using System.Diagnostics;
using System.Text.Json;
using System.Reflection;
using Castle.Core.Internal;

namespace AutomatedTesting.ContractConnectionTests
{
    [TestClass]
    public class QueryWithoutQueryResponseTests
    {
        private const string REPLY_CHANNEL_HEADER = "_QueryReplyChannel";
        private const string QUERY_IDENTIFIER_HEADER = "_QueryClientID";
        private const string REPLY_ID = "_QueryReplyID";

        [TestMethod]
        public async Task TestQueryAsyncWithNoExtendedAspects()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            var responseChannel = "BasicQuery.Response";
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();


            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];
            List<Action<RecievedServiceMessage>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<RecievedServiceMessage>>(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    messages.Add(message);
                    var resp = new RecievedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", responseChannel, message.Header, responseData);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(responseChannel, channels[0]);
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(3, messages[0].Header.Keys.Count());
            Assert.AreEqual(responseChannel, messages[0].Header[REPLY_CHANNEL_HEADER]);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<RecievedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithAHeaderValue()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            var responseChannel = "BasicQuery.Response";
            var headerKey = "MyHeaderKey";
            var headerValue = "MyHeaderValue";
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];
            List<Action<RecievedServiceMessage>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<RecievedServiceMessage>>(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    messages.Add(message);
                    var resp = new RecievedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", responseChannel, message.Header, responseData);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, messageHeader: new([new KeyValuePair<string, string>(headerKey, headerValue)]), responseChannel: responseChannel);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(responseChannel, channels[0]);
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(4, messages[0].Header.Keys.Count());
            Assert.AreEqual(responseChannel, messages[0].Header[REPLY_CHANNEL_HEADER]);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            Assert.AreEqual(1, result.Header.Keys.Count());
            Assert.AreEqual(headerValue, result.Header[headerKey]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<RecievedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithASecondMessageNotMatchingTheRequestBeingSent()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            var responseChannel = "BasicQuery.Response";
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];
            List<Action<RecievedServiceMessage>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<RecievedServiceMessage>>(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    messages.Add(message);
                    var resp = new RecievedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", responseChannel, new([
                        new KeyValuePair<string,string>(QUERY_IDENTIFIER_HEADER,Guid.NewGuid().ToString()),
                        new KeyValuePair<string,string>(REPLY_ID,Guid.NewGuid().ToString()),
                        new KeyValuePair<string,string>(REPLY_CHANNEL_HEADER,responseChannel)
                        ]), responseData);
                    foreach (var action in messageActions)
                        action(resp);
                    resp = new RecievedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", responseChannel, message.Header, responseData);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(responseChannel, channels[0]);
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(3, messages[0].Header.Keys.Count());
            Assert.AreEqual(responseChannel, messages[0].Header[REPLY_CHANNEL_HEADER]);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<RecievedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithTheAttributeChannel()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            var responseChannel = typeof(BasicQueryMessage).GetAttribute<QueryResponseChannelAttribute>()?.Name;
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];
            List<Action<RecievedServiceMessage>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<RecievedServiceMessage>>(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    messages.Add(message);
                    var resp = new RecievedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", responseChannel!, message.Header, responseData);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

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
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(responseChannel, channels[0]);
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(3, messages[0].Header.Keys.Count());
            Assert.AreEqual(responseChannel, messages[0].Header[REPLY_CHANNEL_HEADER]);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<RecievedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncFailingToCreateSubscription()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseChannel = "BasicQuery.Response";

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<RecievedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult<IServiceSubscription?>(null));

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var error = await Assert.ThrowsExceptionAsync<QueryExecutionFailedException>(async()=> await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<RecievedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncFailingWithNoResponseChannel()
        {
            #region Arrange
            var testMessage = new BasicResponseMessage("testMessage");

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<RecievedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult<IServiceSubscription?>(null));

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var error = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await contractConnection.QueryAsync<BasicResponseMessage, BasicResponseMessage>(testMessage, channel: "Test"));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            Assert.AreEqual("responseChannel", error.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<RecievedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithTimeoutException()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseChannel = "BasicQuery.Response";
            var defaultTimeout = TimeSpan.FromSeconds(5);

            var mockSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> messages = [];
            List<Action<RecievedServiceMessage>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<RecievedServiceMessage>>(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    messages.Add(message);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var error = await Assert.ThrowsExceptionAsync<QueryTimeoutException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel, timeout: defaultTimeout));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<RecievedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }
    }
}
