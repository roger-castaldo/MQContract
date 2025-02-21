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
    public class QueryResponseConsumerTests
    {
        [TestMethod()]
        public async ValueTask CheckDefaultRegistrationUsingGenericsAndSuppliedInstance()
        {
            #region Arrange
            var acknowledged = false;

            var serviceSubscription = new Mock<IServiceSubscription>();

            var receivedActions = new List<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>();
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

            var mockConsumer = new Mock<IQueryResponseConsumer<BasicQueryMessage,BasicResponseMessage>>();
            mockConsumer.Setup(x => x.ErrorRecieved(Capture.In(exceptions)));
            mockConsumer.Setup(x => x.MessageReceived(Capture.In(messages)))
                .Returns(new QueryResponseMessage<BasicResponseMessage>(responseMessage));

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var registrationResult = await contractConnection.RegisterQueryResponseConsumerAsync<BasicQueryMessage,BasicResponseMessage, IQueryResponseConsumer<BasicQueryMessage,BasicResponseMessage>>(mockConsumer.Object);
            var result = await contractConnection.QueryAsync<BasicQueryMessage>(message);
            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
            await contractConnection.CloseAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, receivedActions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
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
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
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
            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var mockConsumer = new Mock<IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>>();
            
            var contractConnection = ContractConnection.Instance(serviceConnection.Object);

            var mappedChannel = string.IsNullOrWhiteSpace(channel) ? null : channel;
            var mappedGroup = string.IsNullOrEmpty(group) ? null : group;
            #endregion

            #region Act
            var registrationResult = await contractConnection.RegisterQueryResponseConsumerAsync<BasicQueryMessage,BasicResponseMessage, IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>>(mockConsumer.Object,channel:mappedChannel,group:mappedGroup);
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                mappedChannel??typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
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

            var actions = new List<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>();
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
            var registrationResult = await contractConnection.RegisterQueryResponseConsumerAsync<BasicQueryMessage, BasicResponseMessage, BasicQueryConsumer>();
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            await contractConnection.CloseAsync();
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsNull(groups[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
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

            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);

            var mappedChannel = string.IsNullOrWhiteSpace(channel) ? null : channel;
            var mappedGroup = string.IsNullOrEmpty(group) ? null : group;
            #endregion

            #region Act
            var registrationResult = await contractConnection.RegisterQueryResponseConsumerAsync<BasicQueryMessage, BasicResponseMessage, BasicQueryConsumer>(channel: mappedChannel, group: mappedGroup);
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                mappedChannel??typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
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

            var actions = new List<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>();
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
            var registrationResult = await contractConnection.RegisterQueryResponseConsumerAsync(typeof(BasicQueryConsumer));
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            await contractConnection.CloseAsync();
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsNull(groups[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
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

            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);

            var mappedChannel = string.IsNullOrWhiteSpace(channel) ? null : channel;
            var mappedGroup = string.IsNullOrEmpty(group) ? null : group;
            #endregion

            #region Act
            var registrationResult = await contractConnection.RegisterQueryResponseConsumerAsync(typeof(BasicQueryConsumer),channel: mappedChannel, group: mappedGroup);
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                mappedChannel??typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
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

            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var error = await Assert.ThrowsAsync<InvalidConsumerType>(async()=>await contractConnection.RegisterQueryResponseConsumerAsync(typeof(QueryResponseConsumerTests)));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            Assert.AreEqual($"Unable to register consumer of Type {typeof(QueryResponseConsumerTests).FullName} because it does not implement the interface {typeof(IQueryResponseConsumer<,>).Name}",
                error.Message);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }
    }
}
