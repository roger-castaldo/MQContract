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
    public class PubSubConsumerTests
    {
        [TestMethod()]
        public async ValueTask CheckDefaultRegistrationUsingGenericsAndSuppliedInstance()
        {
            #region Arrange
            var acknowledged = false;

            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Action<ReceivedServiceMessage>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string?>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            serviceSubscription.Setup(x => x.EndAsync())
                .Returns(ValueTask.CompletedTask);

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message, acknowledge: () =>
                    {
                        acknowledged=true;
                        return ValueTask.CompletedTask;
                    });
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var messages = new List<IReceivedMessage<BasicMessage>>();
            var exceptions = new List<Exception>();

            var mockConsumer = new Mock<IPubSubConsumer<BasicMessage>>();
            mockConsumer.Setup(x => x.ErrorRecieved(Capture.In(exceptions)));
            mockConsumer.Setup(x => x.MessageReceived(Capture.In(messages)));

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var registrationResult = await contractConnection.RegisterPubSubConsumerAsync<BasicMessage, IPubSubConsumer<BasicMessage>>(mockConsumer.Object);
            var result = await contractConnection.PublishAsync<BasicMessage>(message);
            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            await contractConnection.CloseAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
            Assert.AreEqual(message, messages[0].Message);
            Assert.AreEqual(exception, exceptions[0]);
            Assert.IsTrue(acknowledged);
            Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].ReceivedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
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
            var serviceConnection = new Mock<IMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var mockConsumer = new Mock<IPubSubConsumer<BasicMessage>>();

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);

            var mappedChannel = string.IsNullOrWhiteSpace(channel) ? null : channel;
            var mappedGroup = string.IsNullOrEmpty(group) ? null : group;
            #endregion

            #region Act
            var registrationResult = await contractConnection.RegisterPubSubConsumerAsync<BasicMessage, IPubSubConsumer<BasicMessage>>(mockConsumer.Object, channel: mappedChannel, group: mappedGroup);
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                mappedChannel??typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
                mappedGroup,
                It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod()]
        public async ValueTask CheckDefaultRegistrationUsingGenericsWithoutInstance()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Action<ReceivedServiceMessage>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string?>();

            serviceSubscription.Setup(x => x.EndAsync())
                .Returns(ValueTask.CompletedTask);

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var registrationResult = await contractConnection.RegisterPubSubConsumerAsync<BasicMessage, BasicMessageConsumer>();
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            await contractConnection.CloseAsync();
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsNull(groups[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
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
            var serviceConnection = new Mock<IMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);

            var mappedChannel = string.IsNullOrWhiteSpace(channel) ? null : channel;
            var mappedGroup = string.IsNullOrEmpty(group) ? null : group;
            #endregion

            #region Act
            var registrationResult = await contractConnection.RegisterPubSubConsumerAsync<BasicMessage, BasicMessageConsumer>(channel: mappedChannel, group: mappedGroup);
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                mappedChannel??typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
                mappedGroup,
                It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod()]
        public async ValueTask CheckDefaultRegistrationUsingType()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Action<ReceivedServiceMessage>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string?>();

            serviceSubscription.Setup(x => x.EndAsync())
                .Returns(ValueTask.CompletedTask);

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var registrationResult = await contractConnection.RegisterPubSubConsumerAsync(typeof(BasicMessageConsumer));
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            await contractConnection.CloseAsync();
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsNull(groups[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
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
            var serviceConnection = new Mock<IMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);

            var mappedChannel = string.IsNullOrWhiteSpace(channel) ? null : channel;
            var mappedGroup = string.IsNullOrEmpty(group) ? null : group;
            #endregion

            #region Act
            var registrationResult = await contractConnection.RegisterPubSubConsumerAsync(typeof(BasicMessageConsumer), channel: mappedChannel, group: mappedGroup);
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                mappedChannel??typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
                mappedGroup,
                It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod()]
        public async ValueTask CheckRegistrationUsingTypeWithInvalidType()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var error = await Assert.ThrowsAsync<InvalidConsumerTypeException>(async () => await contractConnection.RegisterPubSubConsumerAsync(typeof(PubSubConsumerTests)));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            Assert.AreEqual($"Unable to register consumer of Type {typeof(PubSubConsumerTests).FullName} because it does not implement the interface {typeof(IPubSubConsumer<>).Name}",
                error.Message);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod()]
        public async ValueTask CheckDefaultRegistrationUsingConsumerWithIgnoreMessageHeader()
        {
            #region Arrange
            var acknowledged = false;

            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Action<ReceivedServiceMessage>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string?>();
            var serviceMessages = new List<ReceivedServiceMessage>();

            serviceSubscription.Setup(x => x.EndAsync())
                .Returns(ValueTask.CompletedTask);

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message, messageTypeID: "U-DefinitelyNotRight-0.0.0", acknowledge: () =>
                    {
                        acknowledged=true;
                        return ValueTask.CompletedTask;
                    });
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var messages = new List<IReceivedMessage<BasicMessage>>();
            var exceptions = new List<Exception>();

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var registrationResult = await contractConnection.RegisterPubSubConsumerAsync<BasicMessage, BasicMessageConsumerIgnoringMessageType>(new BasicMessageConsumerIgnoringMessageType(messages, exceptions));
            var result = await contractConnection.PublishAsync<BasicMessage>(message);
            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            await contractConnection.CloseAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
            Assert.AreEqual(message, messages[0].Message);
            Assert.AreEqual(exception, exceptions[0]);
            Assert.IsTrue(acknowledged);
            Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].ReceivedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.EndAsync(), Times.Once);
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

            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Action<ReceivedServiceMessage>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string?>();
            var serviceMessages = new List<ReceivedServiceMessage>();
            var publishedMessages = new List<ServiceMessage>();

            serviceSubscription.Setup(x => x.EndAsync())
                .Returns(ValueTask.CompletedTask);

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), Capture.In(errorActions), Capture.In(channels),
                Capture.In(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(publishedMessages), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message, acknowledge: () =>
                    {
                        acknowledged=true;
                        return ValueTask.CompletedTask;
                    });
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var messages = new List<IReceivedMessage<BasicMessage>>();
            var exceptions = new List<Exception>();

            var mockConsumer = new Mock<IPubSubConsumer<BasicMessage>>();
            mockConsumer.Setup(x => x.ErrorRecieved(Capture.In(exceptions)));
            mockConsumer.Setup(x => x.MessageReceived(Capture.In(messages)));

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: withLinking);
            #endregion

            #region Act
            var registrationResult = await contractConnection.RegisterPubSubConsumerAsync<BasicMessage, IPubSubConsumer<BasicMessage>>(mockConsumer.Object);
            var result = await contractConnection.PublishAsync<BasicMessage>(message);
            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            await contractConnection.CloseAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(serviceMessages[0].ReceivedTimestamp, messages[0].ReceivedTimestamp);
            Assert.AreEqual(message, messages[0].Message);
            Assert.AreEqual(exception, exceptions[0]);
            Assert.IsTrue(acknowledged);
            ConnectionHelper.ValidatePublishActivity<BasicMessage>(
                publishedMessages[0],
                capturedActivities[0],
                "MQContract.PublishMessage",
                serviceConnection.Object.GetType(),
                true,
                withLinking
            );
            ConnectionHelper.ValidateConsumeActivity<BasicMessage>(
                serviceMessages[0],
                capturedActivities[1],
                "MQContract.ConsumeMessage",
                serviceConnection.Object.GetType(),
                mockConsumer.Object.GetType(),
                true,
                withLinking
            );
            Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].ReceivedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }
    }
}
