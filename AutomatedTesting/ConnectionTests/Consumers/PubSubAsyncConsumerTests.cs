using AutomatedTesting.Messages;
using Moq;
using MQContract.Attributes;
using MQContract.Interfaces.Service;
using MQContract.Interfaces;
using MQContract;
using System.Diagnostics;
using MQContract.Interfaces.Consumers;
using System.Reflection;
using AutomatedTesting.Consumers;

namespace AutomatedTesting.ConnectionTests.Consumers
{
    [TestClass]
    public class PubSubAsyncConsumerTests
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

            var mockConsumer = new Mock<IPubSubAsyncConsumer<BasicMessage>>();
            mockConsumer.Setup(x => x.ErrorRecieved(Capture.In(exceptions)));
            mockConsumer.Setup(x => x.MessageReceivedAsync(Capture.In(messages)))
                .Returns(ValueTask.CompletedTask);

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var registrationResult = await contractConnection.RegisterPubSubAsyncConsumerAsync<BasicMessage, IPubSubAsyncConsumer<BasicMessage>>(mockConsumer.Object);
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
            serviceSubscription.Verify(x=>x.EndAsync(), Times.Once);    
            #endregion
        }

        [TestMethod()]
        [DataRow("TestChannel",null)]
        [DataRow(null, "TestGroup")]
        public async ValueTask CheckRegistrationUsingGenericsAndSuppliedInstanceWithParameters(string? channel,string? group)
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var mockConsumer = new Mock<IPubSubAsyncConsumer<BasicMessage>>();
            
            var contractConnection = ContractConnection.Instance(serviceConnection.Object);

            var mappedChannel = string.IsNullOrWhiteSpace(channel) ? null : channel;
            var mappedGroup = string.IsNullOrEmpty(group) ? null : group;
            #endregion

            #region Act
            var registrationResult = await contractConnection.RegisterPubSubAsyncConsumerAsync<BasicMessage, IPubSubAsyncConsumer<BasicMessage>>(mockConsumer.Object,channel:mappedChannel,group:mappedGroup);
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
            var registrationResult = await contractConnection.RegisterPubSubAsyncConsumerAsync<BasicMessage, BasicMessageAsyncConsumer>();
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            await contractConnection.CloseAsync();
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name, channels[0]);
            Assert.AreEqual(typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,groups[0]);
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
            var registrationResult = await contractConnection.RegisterPubSubAsyncConsumerAsync<BasicMessage, BasicMessageAsyncConsumer>(channel: mappedChannel, group: mappedGroup);
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                mappedChannel??typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name,
                mappedGroup??typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,
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
            var registrationResult = await contractConnection.RegisterPubSubAsyncConsumerAsync(typeof(BasicMessageAsyncConsumer));
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            await contractConnection.CloseAsync();
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name, channels[0]);
            Assert.AreEqual(typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name, groups[0]);
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
            var registrationResult = await contractConnection.RegisterPubSubAsyncConsumerAsync(typeof(BasicMessageAsyncConsumer),channel: mappedChannel, group: mappedGroup);
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                mappedChannel??typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name,
                mappedGroup??typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,
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
            var error = await Assert.ThrowsAsync<InvalidConsumerType>(async()=>await contractConnection.RegisterPubSubAsyncConsumerAsync(typeof(PubSubAsyncConsumerTests)));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            Assert.AreEqual($"Unable to register consumer of Type {typeof(PubSubAsyncConsumerTests).FullName} because it does not implement the interface {typeof(IPubSubAsyncConsumer<>).Name}",
                error.Message);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
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

            var mockConsumer = new Mock<IPubSubAsyncConsumer<BasicMessage>>();
            mockConsumer.Setup(x => x.ErrorRecieved(Capture.In(exceptions)));
            mockConsumer.Setup(x => x.MessageReceivedAsync(Capture.In(messages)))
                .Returns(ValueTask.CompletedTask);

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: withLinking);
            #endregion

            #region Act
            var registrationResult = await contractConnection.RegisterPubSubAsyncConsumerAsync<BasicMessage, IPubSubAsyncConsumer<BasicMessage>>(mockConsumer.Object);
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
            Assert.AreEqual(2, capturedActivities.Count);
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

        [TestMethod()]
        public async ValueTask CheckDefaultRegistrationUsingGenericsAndSuppliedInstanceWithTelemetryDataWithLinking()
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

            var mockConsumer = new Mock<IPubSubAsyncConsumer<BasicMessage>>();
            mockConsumer.Setup(x => x.ErrorRecieved(Capture.In(exceptions)));
            mockConsumer.Setup(x => x.MessageReceivedAsync(Capture.In(messages)))
                .Returns(ValueTask.CompletedTask);

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: true);
            #endregion

            #region Act
            var registrationResult = await contractConnection.RegisterPubSubAsyncConsumerAsync<BasicMessage, IPubSubAsyncConsumer<BasicMessage>>(mockConsumer.Object);
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
            Assert.AreEqual(2, capturedActivities.Count);
            ConnectionHelper.ValidatePublishActivity<BasicMessage>(
                publishedMessages[0],
                capturedActivities[0],
                "MQContract.PublishMessage",
                serviceConnection.Object.GetType(),
                true,
                true
            );
            ConnectionHelper.ValidateConsumeActivity<BasicMessage>(
                serviceMessages[0],
                capturedActivities[1],
                "MQContract.ConsumeMessage",
                serviceConnection.Object.GetType(),
                mockConsumer.Object.GetType(),
                true,
                true
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
