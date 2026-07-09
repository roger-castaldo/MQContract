using CoreTesting.Messages;
using Moq;
using MQContract;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Service;
using System.Diagnostics;
using System.Reflection;

namespace CoreTesting.ConnectionTests.MultiService;

[TestClass]
public class SubscribeQueryResponseWithoutQueryResponseTest
{
    private const string ServiceName = "testService";

    [TestMethod]
    public async Task TestSubscribeQueryResponseAsyncWithNoExtendedAspects()
    {
        #region Arrange
        var serviceSubscription = new Mock<IServiceSubscription>();
        var serviceSubObject = serviceSubscription.Object;
        var testError = new Exception("this is a test error");

        var channels = new List<string>();
        var groups = new List<string>();
        List<ServiceMessage> messages = [];
        List<Func<ReceivedServiceMessage, ValueTask>> messageActions = [];
        List<Action<Exception>> errorHandlers = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(messageActions), Capture.In<Action<Exception>>(errorHandlers),
            Capture.In(channels), Capture.In(groups), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceSubObject);
        serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
            .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
            {
                messages.Add(message);
                if (Equals(message.Channel, "BasicQueryMessage"))
                    Assert.AreEqual(3, message.Header.Count);
                var idx = channels.IndexOf(message.Channel);
                if (idx != -1)
                    messageActions[idx](new ReceivedServiceMessage(message.ID, message.MessageTypeID, message.Channel, message.Header, message.Data));
                return ValueTask.FromResult(new TransmissionResult(message.ID));
            });

        var contractConnection = ContractConnection.MultiServiceInstance()
            .RegisterServiceConnection(ServiceName, serviceConnection.Object);

        var message = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
        var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
        #endregion

        #region Act
        var receivedMessages = new List<IReceivedMessage<BasicQueryMessage>>();
        var exceptions = new List<Exception>();
        var subscription = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
        {
            receivedMessages.Add(msg);
            return ValueTask.FromResult(new QueryResponseMessage<BasicResponseMessage>(responseMessage, null));
        }, (error) => exceptions.Add(error), cancellationToken: TestContext.CancellationToken);
        var stopwatch = Stopwatch.StartNew();
        var result = await contractConnection.QueryAsync<BasicQueryMessage>(new TransmissionMessage<BasicQueryMessage>(message), cancellationToken: TestContext.CancellationToken);
        stopwatch.Stop();
        Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

        errorHandlers.ForEach(eh => eh(testError));

        await subscription.EndAsync();
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(receivedMessages, 1, TimeSpan.FromMinutes(1)));
        Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(subscription);
        Assert.IsNotNull(result);
        Assert.HasCount(2, channels);
        Assert.HasCount(2, groups);
        Assert.HasCount(2, messages);
        Assert.HasCount(1, exceptions);
        Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
        Assert.IsNull(groups[0]);
        Assert.IsNotNull(groups[1]);
        Assert.AreEqual(receivedMessages[0].ID, messages[0].ID);
        Assert.AreEqual(0, receivedMessages[0].Headers.Count);
        Assert.AreEqual(0, messages[0].Header.Count);
        Assert.AreEqual(message, receivedMessages[0].Message);
        Assert.IsFalse(result.First().IsError);
        Assert.IsNull(result.First().Error);
        Assert.AreEqual(result.First().Result, responseMessage);
        Assert.HasCount(2, errorHandlers);
        Assert.AreEqual(testError, exceptions[0]);
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
           It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        serviceSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
        #endregion
    }

    [TestMethod]
    public async Task TestSubscribeQueryResponseAsyncWithInvalidHeadersOnResponseChannel()
    {
        #region Arrange
        var serviceSubscription = new Mock<IServiceSubscription>();
        var serviceSubObject = serviceSubscription.Object;
        var testError = new Exception("this is a test error");

        var channels = new List<string>();
        var groups = new List<string>();
        List<ServiceMessage> messages = [];
        List<Func<ReceivedServiceMessage, ValueTask>> messageActions = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(messageActions), It.IsAny<Action<Exception>>(),
            Capture.In(channels), Capture.In(groups), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceSubObject);
        serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
            .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
            {
                messages.Add(message);
                var idx = channels.IndexOf(message.Channel);
                if (idx != -1)
                    messageActions[idx](new ReceivedServiceMessage(message.ID, message.MessageTypeID, message.Channel, new([]), message.Data));
                return ValueTask.FromResult(new TransmissionResult(message.ID));
            });

        var contractConnection = ContractConnection.MultiServiceInstance()
            .RegisterServiceConnection(ServiceName, serviceConnection.Object);

        var message = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
        var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
        #endregion

        #region Act
        var receivedMessages = new List<IReceivedMessage<BasicQueryMessage>>();
        var exceptions = new List<Exception>();
        var subscription = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
        {
            receivedMessages.Add(msg);
            return ValueTask.FromResult(new QueryResponseMessage<BasicResponseMessage>(responseMessage, null));
        }, (error) => exceptions.Add(error), cancellationToken: TestContext.CancellationToken);
        var stopwatch = Stopwatch.StartNew();
        var error = await Assert.ThrowsExactlyAsync<QueryTimeoutException>(async () => _ = await contractConnection.QueryAsync<BasicQueryMessage>(new TransmissionMessage<BasicQueryMessage>(message), timeout: TimeSpan.FromSeconds(2), cancellationToken: TestContext.CancellationToken));
        stopwatch.Stop();
        Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

        await subscription.EndAsync();
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(subscription);
        Assert.IsNotNull(error);
        Assert.HasCount(2, channels);
        Assert.HasCount(2, groups);
        Assert.HasCount(1, messages);
        Assert.HasCount(1, exceptions);
        Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
        Assert.IsNull(groups[0]);
        Assert.IsNotNull(groups[1]);
        Assert.IsEmpty(receivedMessages);
        Assert.HasCount(3, messages[0].Header.Keys);
        Assert.IsInstanceOfType<InvalidQueryResponseMessageReceivedException>(exceptions[0]);
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
           It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        serviceSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
        #endregion
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task TestSubscribeQueryResponseAsyncWithTelemetryData(bool withLinking)
    {
        #region Arrange
        (var listener, var capturedActivities, var sourceName) = ConnectionHelper.SetupTelemetry();
        var serviceSubscription = new Mock<IServiceSubscription>();
        var serviceSubObject = serviceSubscription.Object;
        var testError = new Exception("this is a test error");

        var channels = new List<string>();
        var groups = new List<string>();
        List<ServiceMessage> messages = [];
        List<Func<ReceivedServiceMessage, ValueTask>> messageActions = [];
        List<Action<Exception>> errorHandlers = [];
        List<ReceivedServiceMessage> receivedServiceMessages = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(messageActions), Capture.In<Action<Exception>>(errorHandlers),
            Capture.In(channels), Capture.In(groups), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceSubObject);
        serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
            .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
            {
                messages.Add(message);
                var idx = channels.IndexOf(message.Channel);
                var receivedMessage = new ReceivedServiceMessage(message.ID, message.MessageTypeID, message.Channel, message.Header, message.Data);
                receivedServiceMessages.Add(receivedMessage);
                if (idx != -1)
                    messageActions[idx](receivedMessage);
                return ValueTask.FromResult(new TransmissionResult(message.ID));
            });

        var contractConnection = ContractConnection.MultiServiceInstance()
            .RegisterServiceConnection(ServiceName, serviceConnection.Object)
            .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: withLinking);

        var message = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
        var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
        #endregion

        #region Act
        var receivedMessages = new List<IReceivedMessage<BasicQueryMessage>>();
        var exceptions = new List<Exception>();
        var subscription = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
        {
            receivedMessages.Add(msg);
            return ValueTask.FromResult(new QueryResponseMessage<BasicResponseMessage>(responseMessage, null));
        }, (error) => exceptions.Add(error), cancellationToken: TestContext.CancellationToken);
        var stopwatch = Stopwatch.StartNew();
        var result = await contractConnection.QueryAsync<BasicQueryMessage>(new TransmissionMessage<BasicQueryMessage>(message), cancellationToken: TestContext.CancellationToken);
        stopwatch.Stop();
        Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

        errorHandlers.ForEach(eh => eh(testError));

        await subscription.EndAsync();
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(receivedMessages, 1, TimeSpan.FromMinutes(1)));
        Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(subscription);
        Assert.IsNotNull(result);
        Assert.HasCount(2, channels);
        Assert.HasCount(2, groups);
        Assert.HasCount(2, messages);
        Assert.HasCount(1, exceptions);
        Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
        Assert.IsNull(groups[0]);
        Assert.IsNotNull(groups[1]);
        Assert.AreEqual(receivedMessages[0].ID, messages[0].ID);
        Assert.AreEqual((withLinking ? 2 : 0), receivedMessages[0].Headers.Count);
        Assert.AreEqual((withLinking ? 2 : 0), messages[0].Header.Count);
        Assert.AreEqual(message, receivedMessages[0].Message);
        Assert.IsFalse(result.First().IsError);
        Assert.IsNull(result.First().Error);
        Assert.AreEqual(result.First().Result, responseMessage);
        Assert.HasCount(2, errorHandlers);
        Assert.AreEqual(testError, exceptions[0]);
        Assert.HasCount(4, capturedActivities);
        ConnectionHelper.ValidateConsumeActivity<BasicQueryMessage>(
            receivedServiceMessages[0],
            capturedActivities[1],
            "MQContract.ConsumeQueryMessage",
            serviceConnection.Object.GetType(),
            true,
            withLinking,
            connectionName: ServiceName
        );
        ConnectionHelper.ValidatePublishActivity<BasicResponseMessage>(
            messages[1],
            capturedActivities[2],
            "MQContract.ProduceQueryResponse",
            serviceConnection.Object.GetType(),
            true,
            withLinking,
            connectionName: ServiceName
        );
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
           It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        serviceSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
        #endregion
    }

    [TestMethod]
    [DataRow("headerValue", "differentHeaderValue", "messageHeaderValue", "messageHeaderValue", "messageValue", "messageValue", true)]
    [DataRow("headerValue", "differentHeaderValue", "messageHeaderValue", "messageHeaderValue", "messageValue", "messageValue", false)]
    [DataRow("headerValue", "headerValue", "messageHeaderValue", "differentMessageHeaderValue", "messageValue", "messageValue", true)]
    [DataRow("headerValue", "headerValue", "messageHeaderValue", "differentMessageHeaderValue", "messageValue", "messageValue", false)]
    [DataRow("headerValue", "headerValue", "messageHeaderValue", "messageHeaderValue", "messageValue", "differentMessageValue", true)]
    [DataRow("headerValue", "headerValue", "messageHeaderValue", "messageHeaderValue", "messageValue", "differentMessageValue", false)]
    [DataRow("headerValue", "headerValue", "messageHeaderValue", "messageHeaderValue", "messageValue", "messageValue", true)]
    public async Task TestSubscribeAsyncWithFiltering(string headerValue, string checkValue, string messageHeaderValue, string messageHeaderCheckValue,
        string messageValue, string messageCheckValue, bool acknowledgeDrop)
    {
        #region Arrange
        var headerKey = "testHeader";
        var messageHeaderKey = "testMessageHeader";
        var acknowledged = false;

        var serviceSubscription = new Mock<IServiceSubscription>();
        var serviceSubObject = serviceSubscription.Object;

        var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
        var channels = new List<string>();
        var serviceMessages = new List<ServiceMessage>();

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.SubscribeAsync(
            Capture.In(actions),
            It.IsAny<Action<Exception>>(), Capture.In<string>(channels),
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceSubObject);
        serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
            .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
            {
                serviceMessages.Add(message);
                var idx = channels.IndexOf(message.Channel);
                if (idx != -1)
                    actions[idx](Helper.ProduceReceivedServiceMessage(message, acknowledge: () =>
                    {
                        acknowledged=true;
                        return ValueTask.CompletedTask;
                    }));
                return ValueTask.FromResult(new TransmissionResult(message.ID));
            });

        var contractConnection = ContractConnection.MultiServiceInstance()
            .RegisterServiceConnection(ServiceName, serviceConnection.Object);

        var message = new BasicQueryMessage(messageValue);
        var responseMessage = new BasicResponseMessage(messageValue);
        #endregion

        #region Act
        var messages = new List<IReceivedMessage<BasicQueryMessage>>();
        var subscription = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
        {
            messages.Add(msg);
            return ValueTask.FromResult(new QueryResponseMessage<BasicResponseMessage>(responseMessage, null));
        }, (error) => { },
        messageFilters: new(
            HeaderFilter: (header) =>
                ValueTask.FromResult<MessageFilterResult>((Equals(header[headerKey], checkValue), acknowledgeDrop) switch
                {
                    (true, _) => MessageFilterResult.Allow,
                    (false, false) => MessageFilterResult.DropAndDontAcknowledge,
                    (false, true) => MessageFilterResult.DropAndAcknowledge
                }),
            MessageFilter: (serviceMessage, header) =>
                ValueTask.FromResult<MessageFilterResult>((Equals(header[messageHeaderKey], messageHeaderCheckValue), Equals(serviceMessage.TypeName, messageCheckValue), acknowledgeDrop) switch
                {
                    (true, true, _) => MessageFilterResult.Allow,
                    (false, _, false) => MessageFilterResult.DropAndDontAcknowledge,
                    (false, _, true) => MessageFilterResult.DropAndAcknowledge,
                    (_, false, false) => MessageFilterResult.DropAndDontAcknowledge,
                    (_, false, true) => MessageFilterResult.DropAndAcknowledge,
                })
        ), cancellationToken: TestContext.CancellationToken);
        IEnumerable<QueryResult<object>> result = [];
        Exception? error = null;
        var messageHeader = new MessageHeader([
            new KeyValuePair<string,string?>(headerKey,headerValue),
            new KeyValuePair<string,string?>(messageHeaderKey,messageHeaderValue)
        ]);
        if (Equals(headerValue, checkValue) && Equals(messageHeaderValue, messageHeaderCheckValue) && Equals(messageValue, messageCheckValue))
            result = await contractConnection.QueryAsync<BasicQueryMessage>(new TransmissionMessage<BasicQueryMessage>(message, Header: messageHeader), timeout: TimeSpan.FromMilliseconds(500), cancellationToken: TestContext.CancellationToken);
        else
            error = await Assert.ThrowsAsync<Exception>(async () => _ = await contractConnection.QueryAsync<BasicQueryMessage>(new TransmissionMessage<BasicQueryMessage>(message, Header: messageHeader), timeout: TimeSpan.FromMilliseconds(500), cancellationToken: TestContext.CancellationToken));
        #endregion

        #region Assert
        var publishCount = 1;
        Assert.IsNotNull(subscription);
        Assert.HasCount(2, actions);
        if (Equals(headerValue, checkValue) && Equals(messageHeaderValue, messageHeaderCheckValue) && Equals(messageValue, messageCheckValue))
        {
            publishCount=2;
            Assert.HasCount(2, serviceMessages);
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotEmpty(result);
            Assert.IsNull(error);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Count, messages[0].Headers.Count);
            Assert.AreEqual(message, messages[0].Message);
        }
        else
        {
            Assert.HasCount(1, serviceMessages);
            Assert.IsEmpty(result);
            Assert.IsNotNull(error);
            Assert.IsInstanceOfType<QueryTimeoutException>(error);
            Assert.IsEmpty(messages);
        }
        Assert.AreEqual(acknowledgeDrop, acknowledged);
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(publishCount));
        serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(),
           It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        #endregion
    }

    public TestContext TestContext { get; set; }
}
