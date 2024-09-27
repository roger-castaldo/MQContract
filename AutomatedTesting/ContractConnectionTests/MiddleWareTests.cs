using Moq;
using MQContract.Interfaces.Service;
using MQContract;
using AutomatedTesting.Messages;
using AutomatedTesting.ContractConnectionTests.Middlewares;
using MQContract.Interfaces.Middleware;
using MQContract.Interfaces;

namespace AutomatedTesting.ContractConnectionTests
{
    [TestClass]
    public class MiddleWareTests
    {
        [TestMethod]
        public async Task TestRegisterGenericMiddleware()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var messageChannel = "TestRegisterGenericMiddleware";

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .RegisterMiddleware<ChannelChangeMiddleware>();
            #endregion

            #region Act
            _ = await contractConnection.PublishAsync<BasicMessage>(testMessage,channel:messageChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.AreEqual(messages[0].Channel, ChannelChangeMiddleware.ChangeChannel(messageChannel));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterGenericMiddlewareThroughFunction()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var messageChannel = "TestRegisterGenericMiddlewareThrouwFunction";
            var newChannel = "NewTestRegisterGenericMiddlewareThrouwFunction";
            var headers = new MessageHeader([
                new KeyValuePair<string,string>("test","test")
            ]);

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var mockMiddleware = new Mock<IBeforeEncodeMiddleware>();
            mockMiddleware.Setup(x => x.BeforeMessageEncodeAsync(It.IsAny<IContext>(), It.IsAny<BasicMessage>(), It.IsAny<string?>(), It.IsAny<MessageHeader>()))
                .Returns((IContext context, BasicMessage message, string? channel, MessageHeader messageHeader) =>
                {
                    return ValueTask.FromResult<(BasicMessage message, string? channel, MessageHeader messageHeader)>((message,newChannel,headers));
                });


            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .RegisterMiddleware<IBeforeEncodeMiddleware>(() => mockMiddleware.Object);
            #endregion

            #region Act
            _ = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: messageChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.AreEqual(messages[0].Channel, newChannel);
            Assert.AreEqual(1, messages[0].Header.Keys.Count());
            Assert.AreEqual(headers[headers.Keys.First()], messages[0].Header[messages[0].Header.Keys.First()]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMiddleware.Verify(x => x.BeforeMessageEncodeAsync(It.IsAny<IContext>(), It.IsAny<BasicMessage>(), It.IsAny<string?>(), It.IsAny<MessageHeader>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterSpecificTypeMiddleware()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var messageChannel = "TestRegisterSpecificTypeMiddleware";

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .RegisterMiddleware<ChannelChangeMiddlewareForBasicMessage,BasicMessage>();
            #endregion

            #region Act
            _ = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: messageChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.AreEqual(messages[0].Channel, ChannelChangeMiddlewareForBasicMessage.ChangeChannel(messageChannel));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterSpecificTypeMiddlewareThroughFunction()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var messageChannel = "TestRegisterSpecificTypeMiddlewareThroughFunction";
            var newChannel = "NewTestRegisterSpecificTypeMiddlewareThroughFunction";
            var headers = new MessageHeader([
                new KeyValuePair<string,string>("test","test")
            ]);

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var mockMiddleware = new Mock<IBeforeEncodeSpecificTypeMiddleware<BasicMessage>>();
            mockMiddleware.Setup(x => x.BeforeMessageEncodeAsync(It.IsAny<IContext>(), It.IsAny<BasicMessage>(), It.IsAny<string?>(), It.IsAny<MessageHeader>()))
                .Returns((IContext context, BasicMessage message, string? channel, MessageHeader messageHeader) =>
                {
                    return ValueTask.FromResult((message, newChannel, headers));
                });


            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .RegisterMiddleware<IBeforeEncodeSpecificTypeMiddleware<BasicMessage>,BasicMessage>(() => mockMiddleware.Object);
            #endregion

            #region Act
            _ = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: messageChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.AreEqual(messages[0].Channel, newChannel);
            Assert.AreEqual(1, messages[0].Header.Keys.Count());
            Assert.AreEqual(headers[headers.Keys.First()], messages[0].Header[messages[0].Header.Keys.First()]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMiddleware.Verify(x => x.BeforeMessageEncodeAsync(It.IsAny<IContext>(), It.IsAny<BasicMessage>(), It.IsAny<string?>(), It.IsAny<MessageHeader>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterGenericMiddlewareWithService()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            var messageChannel = "TestRegisterGenericMiddleware";
            var expectedChannel = "TestRegisterGenericMiddlewareWithService";

            var services = Helper.ProduceServiceProvider(expectedChannel);

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, serviceProvider: services)
                .RegisterMiddleware<InjectedChannelChangeMiddleware>();
            #endregion

            #region Act
            _ = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: messageChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.AreEqual(messages[0].Channel, expectedChannel);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestRegisterSpecificTypeMiddlewarePostDecodingThroughFunction()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();

            var testMessage = new BasicMessage("testMessage");
            var messageChannel = "TestRegisterSpecificTypeMiddlewareThroughFunction";
            var headers = new MessageHeader([
                new KeyValuePair<string,string>("test","test")
            ]);

            var actions = new List<Action<ReceivedServiceMessage>>();

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Moq.Capture.In<Action<ReceivedServiceMessage>>(actions),It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var mockMiddleware = new Mock<IAfterDecodeSpecificTypeMiddleware<BasicMessage>>();
            mockMiddleware.Setup(x => x.AfterMessageDecodeAsync(It.IsAny<IContext>(),It.IsAny<BasicMessage>(),It.IsAny<string>(),It.IsAny<MessageHeader>(),It.IsAny<DateTime>(),It.IsAny<DateTime>()))
                .Returns((IContext context, BasicMessage message, string ID, MessageHeader messageHeader,DateTime recievedTimestamp,DateTime processedTimeStamp) =>
                {
                    return ValueTask.FromResult((message,headers));
                });


            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .RegisterMiddleware<IAfterDecodeSpecificTypeMiddleware<BasicMessage>, BasicMessage>(() => mockMiddleware.Object);
            #endregion

            #region Act
            var messages = new List<IReceivedMessage<BasicMessage>>();
            _ = await contractConnection.SubscribeAsync<BasicMessage>((msg) =>
            {
                messages.Add(msg);
                return ValueTask.CompletedTask;
            }, (error) => { });
            _ = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: messageChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<IReceivedMessage<BasicMessage>>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.AreEqual(headers.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(headers[headers.Keys.First()], messages[0].Headers[messages[0].Headers.Keys.First()]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMiddleware.Verify(x => x.AfterMessageDecodeAsync(It.IsAny<IContext>(), It.IsAny<BasicMessage>(), It.IsAny<string>(), It.IsAny<MessageHeader>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            #endregion
        }
    }
}
