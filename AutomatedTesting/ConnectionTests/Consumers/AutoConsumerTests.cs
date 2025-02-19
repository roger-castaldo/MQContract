using AutomatedTesting.Messages;
using Moq;
using MQContract.Attributes;
using MQContract.Interfaces.Service;
using MQContract;
using System.Reflection;
using AutomatedTesting.Consumers;

namespace AutomatedTesting.ConnectionTests.Consumers
{
    [TestClass]
    public class AutoConsumerTests
    {
        [TestMethod]
        public async ValueTask CheckLoadConsumersWithAssembly()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            serviceSubscription.Setup(x => x.EndAsync())
                .Returns(ValueTask.CompletedTask);

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x=>x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);  

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var registrationResult = await contractConnection.AutoRegisterAllConsumersAsync(GetType().Assembly);
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            await contractConnection.DisposeAsync();
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), 
                    typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name, 
                    null, It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name,
                    typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,
                    It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
                    null, It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicQueryAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name,
                    typeof(BasicQueryAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,
                    It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.EndAsync(), Times.Exactly(4));
            #endregion
        }

        [TestMethod]
        public async ValueTask CheckLoadConsumersWithoutAssembly()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            serviceSubscription.Setup(x => x.EndAsync())
                .Returns(ValueTask.CompletedTask);

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var registrationResult = await contractConnection.AutoRegisterAllConsumersAsync();
            #endregion

            #region Assert
            Assert.IsTrue(registrationResult);
            await contractConnection.DisposeAsync();
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
                    null, It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name,
                    typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,
                    It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
                    null, It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicQueryAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name,
                    typeof(BasicQueryAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,
                    It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.EndAsync(), Times.Exactly(4));
            #endregion
        }

        [TestMethod]
        public async ValueTask CheckLoadConsumerWithFailureToCreateSubscriptionForPubSubConsumer()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
                    null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IServiceSubscription?)null);
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name,
                    typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
                    null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicQueryAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name,
                    typeof(BasicQueryAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var registrationResult = await contractConnection.AutoRegisterAllConsumersAsync(GetType().Assembly);
            #endregion

            #region Assert
            Assert.IsFalse(registrationResult);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
                    null, It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async ValueTask CheckLoadConsumerWithFailureToCreateSubscriptionForPubSubAsyncConsumer()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
                    null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name,
                    typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IServiceSubscription?)null);
            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
                    null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicQueryAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name,
                    typeof(BasicQueryAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var registrationResult = await contractConnection.AutoRegisterAllConsumersAsync(GetType().Assembly);
            #endregion

            #region Assert
            Assert.IsFalse(registrationResult);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name,
                    typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,
                    It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async ValueTask CheckLoadConsumerWithFailureToCreateSubscriptionForQueryResponseConsumer()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
                    null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name,
                    typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
                    null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IServiceSubscription?)null);
            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicQueryAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name,
                    typeof(BasicQueryAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var registrationResult = await contractConnection.AutoRegisterAllConsumersAsync(GetType().Assembly);
            #endregion

            #region Assert
            Assert.IsFalse(registrationResult);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
                    null, It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async ValueTask CheckLoadConsumerWithFailureToCreateSubscriptionForQueryResponseAsyncConsumer()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
                    null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name,
                    typeof(BasicMessageAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)!.Name,
                    null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicQueryAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name,
                    typeof(BasicQueryAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IServiceSubscription?)null);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var registrationResult = await contractConnection.AutoRegisterAllConsumersAsync(GetType().Assembly);
            #endregion

            #region Assert
            Assert.IsFalse(registrationResult);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(),
                    typeof(BasicQueryAsyncConsumer).GetCustomAttribute<ConsumerMessageChannelAttribute>(false)!.Name,
                    typeof(BasicQueryAsyncConsumer).GetCustomAttribute<ConsumerGroupAttribute>(false)!.Name,
                    It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }
    }
}
