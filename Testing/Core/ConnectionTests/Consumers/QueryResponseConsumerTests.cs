using AutomatedTesting.Consumers;
using AutomatedTesting.Messages;
using Moq;
using MQContract;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;
using MQContract.Interfaces.Service;
using System.Diagnostics;
using System.Reflection;

namespace AutomatedTesting.ConnectionTests.Consumers
{
    [TestClass]
    public class QueryResponseConsumerTests
    {
        [TestMethod()]
        public async ValueTask CheckDefaultRegistrationUsingGenericsAndSuppliedInstance()
        {
            #region Arrange
            var acknowledged = false;

            var serviceSubscription = new Mock<IServiceSubscription>();

            var receivedActions = new List<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string?>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            serviceSubscription.Setup(x => x.EndAsync())
                .Returns(ValueTask.CompletedTask);

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                Capture.In(receivedActions),
                Capture.In(errorActions),
                Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(async (ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message, acknowledge: () =>
                    {
                        acknowledged=true;
                        return ValueTask.CompletedTask;
                    });
                    serviceMessages.Add(rmessage);
                    var result = await receivedActions[0](rmessage);
                    return Helper.ProduceQueryResult(result);
                });

            var messages = new List<IReceivedMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();

            var message = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeQueryResponseWithNoExtendedAspects");

            var mockConsumer = new Mock<IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>>();
            mockConsumer.Setup(x => x.ErrorRecieved(Capture.In(exceptions)));
            mockConsumer.Setup(x => x.MessageReceived(Capture.In(messages)))
                .Returns(new QueryResponseMessage<BasicResponseMessage>(responseMessage));

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            contractConnection = await contractConnection.RegisterQueryResponseConsumerAsync<BasicQueryMessage, BasicResponseMessage, IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>>(mockConsumer.Object, cancellationToken: TestContext.CancellationToken);
            var result = await contractConnection.QueryAsync<BasicQueryMessage>(message, cancellationToken: TestContext.CancellationToken);
            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            await contractConnection.CloseAsync();
            Assert.IsNotNull(result);
            Assert.HasCount(1, receivedActions);
            Assert.HasCount(1, channels);
            Assert.HasCount(1, groups);
            Assert.HasCount(1, serviceMessages);
            Assert.HasCount(1, errorActions);
            Assert.HasCount(1, exceptions);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
            Assert.AreEqual(message, messages[0].Message);
            Assert.AreEqual(exception, exceptions[0]);
            Assert.IsFalse(result.IsError);
            Assert.IsNull(result.Error);
            Assert.AreEqual(result.Result, responseMessage);
            Assert.IsTrue(acknowledged);
            Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].ReceivedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod()]
        [DataRow("TestChannel", null)]
        [DataRow(null, "TestGroup")]
        public async ValueTask CheckRegistrationUsingGenericsAndSuppliedInstanceWithParameters(string? channel, string? group)
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var mockConsumer = new Mock<IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>>();

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);

            var mappedChannel = string.IsNullOrWhiteSpace(channel) ? null : channel;
            var mappedGroup = string.IsNullOrEmpty(group) ? null : group;
            #endregion

            #region Act
            _ = await contractConnection.RegisterQueryResponseConsumerAsync<BasicQueryMessage, BasicResponseMessage, IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>>(mockConsumer.Object, channel: mappedChannel, group: mappedGroup, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>(), It.IsAny<Action<Exception>>(),
                mappedChannel??typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)!.Channel!,
                mappedGroup,
                It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod()]
        public async ValueTask CheckDefaultRegistrationUsingGenericsWithoutInstance()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            var actions = new List<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string?>();

            serviceSubscription.Setup(x => x.EndAsync())
                .Returns(ValueTask.CompletedTask);

            serviceConnection.Setup(x => x.SubscribeQueryAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            _ = await contractConnection.RegisterQueryResponseConsumerAsync<BasicQueryMessage, BasicResponseMessage, BasicQueryConsumer>(cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            await contractConnection.CloseAsync();
            Assert.HasCount(1, actions);
            Assert.HasCount(1, channels);
            Assert.HasCount(1, groups);
            Assert.HasCount(1, errorActions);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
            Assert.IsNull(groups[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod()]
        [DataRow("TestChannel", "")]
        [DataRow("", "TestGroup")]
        public async ValueTask CheckRegistrationUsingGenericsWithoutInstanceWithParameters(string channel, string group)
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);

            var mappedChannel = string.IsNullOrWhiteSpace(channel) ? null : channel;
            var mappedGroup = string.IsNullOrEmpty(group) ? null : group;
            #endregion

            #region Act
            _ = await contractConnection.RegisterQueryResponseConsumerAsync<BasicQueryMessage, BasicResponseMessage, BasicQueryConsumer>(channel: mappedChannel, group: mappedGroup, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>(), It.IsAny<Action<Exception>>(),
                mappedChannel??typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)!.Channel!,
                mappedGroup,
                It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod()]
        public async ValueTask CheckDefaultRegistrationUsingType()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            var actions = new List<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string?>();

            serviceSubscription.Setup(x => x.EndAsync())
                .Returns(ValueTask.CompletedTask);

            serviceConnection.Setup(x => x.SubscribeQueryAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            _ = await contractConnection.RegisterQueryResponseConsumerAsync(typeof(BasicQueryConsumer), cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            await contractConnection.CloseAsync();
            Assert.HasCount(1, actions);
            Assert.HasCount(1, channels);
            Assert.HasCount(1, groups);
            Assert.HasCount(1, errorActions);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
            Assert.IsNull(groups[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod()]
        [DataRow("TestChannel", "")]
        [DataRow("", "TestGroup")]
        public async ValueTask CheckRegistrationUsingTypeWithParameters(string channel, string group)
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);

            var mappedChannel = string.IsNullOrWhiteSpace(channel) ? null : channel;
            var mappedGroup = string.IsNullOrEmpty(group) ? null : group;
            #endregion

            #region Act
            _ = await contractConnection.RegisterQueryResponseConsumerAsync(typeof(BasicQueryConsumer), channel: mappedChannel, group: mappedGroup, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>(), It.IsAny<Action<Exception>>(),
                mappedChannel??typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)!.Channel!,
                mappedGroup,
                It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod()]
        public async ValueTask CheckRegistrationUsingTypeWithInvalidType()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var error = await Assert.ThrowsAsync<InvalidConsumerTypeException>(async () => await contractConnection.RegisterQueryResponseConsumerAsync(typeof(QueryResponseConsumerTests), cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            Assert.AreEqual($"Unable to register consumer of Type {typeof(QueryResponseConsumerTests).FullName} because it does not implement the interface {typeof(IQueryResponseConsumer<,>).Name}",
                error.Message);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod()]
        [DataRow(false)]
        [DataRow(true)]
        public async ValueTask CheckDefaultRegistrationUsingGenericsAndSuppliedInstanceWithTelemetryData(bool withLinking)
        {
            #region Arrange
            (var listener, var capturedActivities, var sourceName) = ConnectionHelper.SetupTelemetry();
            var acknowledged = false;

            var serviceSubscription = new Mock<IServiceSubscription>();

            var receivedActions = new List<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string?>();
            var serviceMessages = new List<ReceivedServiceMessage>();
            ServiceQueryResult? queryResult = null;

            serviceSubscription.Setup(x => x.EndAsync())
                .Returns(ValueTask.CompletedTask);

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                Capture.In(receivedActions),
                Capture.In(errorActions),
                Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(async (ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message, acknowledge: () =>
                    {
                        acknowledged=true;
                        return ValueTask.CompletedTask;
                    });
                    serviceMessages.Add(rmessage);
                    var result = await receivedActions[0](rmessage);
                    queryResult = Helper.ProduceQueryResult(result);
                    return queryResult!;
                });

            var messages = new List<IReceivedMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();

            var message = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeQueryResponseWithNoExtendedAspects");

            var mockConsumer = new Mock<IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>>();
            mockConsumer.Setup(x => x.ErrorRecieved(Capture.In(exceptions)));
            mockConsumer.Setup(x => x.MessageReceived(Capture.In(messages)))
                .Returns(new QueryResponseMessage<BasicResponseMessage>(responseMessage));

            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: withLinking);
            #endregion

            #region Act
            contractConnection = await contractConnection.RegisterQueryResponseConsumerAsync<BasicQueryMessage, BasicResponseMessage, IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>>(mockConsumer.Object, cancellationToken: TestContext.CancellationToken);
            var result = await contractConnection.QueryAsync<BasicQueryMessage>(message, cancellationToken: TestContext.CancellationToken);
            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            await contractConnection.CloseAsync();
            Assert.IsNotNull(result);
            Assert.HasCount(1, receivedActions);
            Assert.HasCount(1, channels);
            Assert.HasCount(1, groups);
            Assert.HasCount(1, serviceMessages);
            Assert.HasCount(1, errorActions);
            Assert.HasCount(1, exceptions);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
            Assert.AreEqual(message, messages[0].Message);
            Assert.AreEqual(exception, exceptions[0]);
            Assert.IsFalse(result.IsError);
            Assert.IsNull(result.Error);
            Assert.AreEqual(result.Result, responseMessage);
            Assert.IsTrue(acknowledged);
            Assert.HasCount(4, capturedActivities);
            ConnectionHelper.ValidateConsumeActivity<BasicQueryMessage>(
                serviceMessages[0],
                capturedActivities[1],
                "MQContract.ConsumeQueryMessage",
                serviceConnection.Object.GetType(),
                mockConsumer.Object.GetType(),
                true,
                withLinking
            );
            ConnectionHelper.ValidatePublishActivity<BasicResponseMessage>(
                queryResult!,
                capturedActivities[2],
                "MQContract.ProduceQueryResponse",
                serviceConnection.Object.GetType(),
                true,
                withLinking
            );
            Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].ReceivedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.EndAsync(), Times.Once);
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
        public async Task TestRegisterConsumerWithFiltering(string headerValue, string checkValue, string messageHeaderValue, string messageHeaderCheckValue,
            string messageValue, string messageCheckValue, bool acknowledgeDrop)
        {
            #region Arrange
            var headerKey = "testHeader";
            var messageHeaderKey = "testMessageHeader";
            var acknowledged = false;
            var droppedException = new Exception("Message Dropped");

            var serviceSubscription = new Mock<IServiceSubscription>();

            var actions = new List<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                Capture.In(actions),
                It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(async (ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message, acknowledge: () =>
                    {
                        acknowledged=true;
                        return ValueTask.CompletedTask;
                    });
                    serviceMessages.Add(rmessage);
                    var result = await actions[0](rmessage);
                    if (result!=null)
                        return Helper.ProduceQueryResult(result);
                    throw droppedException;
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);

            var message = new BasicQueryMessage(messageValue);
            var responseMessage = new BasicResponseMessage(messageValue);

            var messages = new List<IReceivedMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();

            var mockConsumer = new Mock<IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>>();
            mockConsumer.Setup(x => x.ErrorRecieved(Capture.In(exceptions)));
            mockConsumer.Setup(x => x.MessageReceived(Capture.In(messages)))
                .Returns(new QueryResponseMessage<BasicResponseMessage>(responseMessage));
            #endregion

            #region Act
            contractConnection = await contractConnection.RegisterQueryResponseConsumerAsync<BasicQueryMessage, BasicResponseMessage, IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>>(mockConsumer.Object,
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
            QueryResult<object>? result = null;
            Exception? error = null;
            var messageHeader = new MessageHeader([
                new KeyValuePair<string,string>(headerKey,headerValue),
                new KeyValuePair<string,string>(messageHeaderKey,messageHeaderValue)
            ]);
            if (Equals(headerValue, checkValue) && Equals(messageHeaderValue, messageHeaderCheckValue) && Equals(messageValue, messageCheckValue))
                result = await contractConnection.QueryAsync<BasicQueryMessage>(message, messageHeader: messageHeader, timeout: TimeSpan.FromMilliseconds(500), cancellationToken: TestContext.CancellationToken);
            else
                error = await Assert.ThrowsAsync<Exception>(async () => _ = await contractConnection.QueryAsync<BasicQueryMessage>(message, messageHeader: messageHeader, timeout: TimeSpan.FromMilliseconds(500), cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Assert
            Assert.HasCount(1, actions);
            Assert.HasCount(1, serviceMessages);
            if (Equals(headerValue, checkValue) && Equals(messageHeaderValue, messageHeaderCheckValue) && Equals(messageValue, messageCheckValue))
            {
                Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
                Assert.IsNotNull(result);
                Assert.IsNull(error);
                Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
                Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
                Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
                Assert.AreEqual(message, messages[0].Message);
            }
            else
            {
                Assert.IsNull(result);
                Assert.AreEqual(error, droppedException);
                Assert.IsEmpty(messages);
            }
            Assert.AreEqual(acknowledgeDrop, acknowledged);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
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
        public async Task TestRegisterConsumerWithFilteredConsumer(string headerValue, string checkValue, string messageHeaderValue, string messageHeaderCheckValue,
            string messageValue, string messageCheckValue, bool acknowledgeDrop)
        {
            #region Arrange
            var headerKey = "testHeader";
            var messageHeaderKey = "testMessageHeader";
            var acknowledged = false;
            var droppedException = new Exception("Message Dropped");

            var serviceSubscription = new Mock<IServiceSubscription>();

            var actions = new List<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                Capture.In(actions),
                It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(async (ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message, acknowledge: () =>
                    {
                        acknowledged=true;
                        return ValueTask.CompletedTask;
                    });
                    serviceMessages.Add(rmessage);
                    var result = await actions[0](rmessage);
                    if (result!=null)
                        return Helper.ProduceQueryResult(result);
                    throw droppedException;
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);

            var message = new BasicQueryMessage(messageValue);
            var responseMessage = new BasicResponseMessage(messageValue);

            var messages = new List<IReceivedMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();

            var mockConsumer = new Mock<IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>>();
            mockConsumer.Setup(x => x.ErrorRecieved(Capture.In(exceptions)));
            mockConsumer.Setup(x => x.MessageReceived(Capture.In(messages)))
                .Returns(new QueryResponseMessage<BasicResponseMessage>(responseMessage));

            if (!Equals(headerValue, checkValue))
                mockConsumer.As<IHeaderFilteredConsumer>().SetupGet(x => x.Filter)
                    .Returns((MessageHeader header) => ValueTask.FromResult<MessageFilterResult>((Equals(header[headerKey], checkValue), acknowledgeDrop) switch
                    {
                        (true, _) => MessageFilterResult.Allow,
                        (false, false) => MessageFilterResult.DropAndDontAcknowledge,
                        (false, true) => MessageFilterResult.DropAndAcknowledge
                    }));
            if (Equals(headerValue, checkValue))
                mockConsumer.As<IMessageFilteredConsumer<BasicQueryMessage>>().SetupGet(x => x.Filter)
                    .Returns((BasicQueryMessage serviceMessage, MessageHeader header) => ValueTask.FromResult<MessageFilterResult>((Equals(header[messageHeaderKey], messageHeaderCheckValue), Equals(serviceMessage.TypeName, messageCheckValue), acknowledgeDrop) switch
                    {
                        (true, true, _) => MessageFilterResult.Allow,
                        (false, _, false) => MessageFilterResult.DropAndDontAcknowledge,
                        (false, _, true) => MessageFilterResult.DropAndAcknowledge,
                        (_, false, false) => MessageFilterResult.DropAndDontAcknowledge,
                        (_, false, true) => MessageFilterResult.DropAndAcknowledge,
                    }));
            #endregion

            #region Act
            contractConnection = await contractConnection.RegisterQueryResponseConsumerAsync<BasicQueryMessage, BasicResponseMessage, IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>>(mockConsumer.Object, cancellationToken: TestContext.CancellationToken);
            QueryResult<object>? result = null;
            Exception? error = null;
            var messageHeader = new MessageHeader([
                new KeyValuePair<string,string>(headerKey,headerValue),
                new KeyValuePair<string,string>(messageHeaderKey,messageHeaderValue)
            ]);
            if (Equals(headerValue, checkValue) && Equals(messageHeaderValue, messageHeaderCheckValue) && Equals(messageValue, messageCheckValue))
                result = await contractConnection.QueryAsync<BasicQueryMessage>(message, messageHeader: messageHeader, timeout: TimeSpan.FromMilliseconds(500), cancellationToken: TestContext.CancellationToken);
            else
                error = await Assert.ThrowsAsync<Exception>(async () => _ = await contractConnection.QueryAsync<BasicQueryMessage>(message, messageHeader: messageHeader, timeout: TimeSpan.FromMilliseconds(500), cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Assert
            Assert.HasCount(1, actions);
            Assert.HasCount(1, serviceMessages);
            if (Equals(headerValue, checkValue) && Equals(messageHeaderValue, messageHeaderCheckValue) && Equals(messageValue, messageCheckValue))
            {
                Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
                Assert.IsNotNull(result);
                Assert.IsNull(error);
                Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
                Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
                Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
                Assert.AreEqual(message, messages[0].Message);
            }
            else
            {
                Assert.IsNull(result);
                Assert.AreEqual(error, droppedException);
                Assert.IsEmpty(messages);
            }
            Assert.AreEqual(acknowledgeDrop, acknowledged);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        public TestContext TestContext { get; set; }
    }
}
