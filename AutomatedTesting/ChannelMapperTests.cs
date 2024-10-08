﻿using AutomatedTesting.Messages;
using Moq;
using MQContract.Attributes;
using MQContract.Interfaces.Service;
using MQContract;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace AutomatedTesting
{
    [TestClass]
    public class ChannelMapperTests
    {
        [TestMethod]
        public async Task TestPublishMapWithStringToString()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var newChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var mapper = new ChannelMapper()
                .AddPublishMap(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name!, newChannel);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object,channelMapper:mapper);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(newChannel, messages[0].Channel);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishMapWithStringToFunction()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");

            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);

            var newChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var otherChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddPublishMap(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name!, (originalChannel) =>
                {
                    if (Equals(originalChannel, typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name))
                        return ValueTask.FromResult(newChannel);
                    return ValueTask.FromResult(originalChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.PublishAsync<BasicMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            var result2 = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: otherChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.AreEqual(transmissionResult, result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(transmissionResult, result2);
            Assert.AreEqual(newChannel, messages[0].Channel);
            Assert.AreEqual(otherChannel, messages[1].Channel); 
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestPublishMapWithMatchToFunction()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            
            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            
            var newChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var otherChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddPublishMap(
                (channelName)=>Equals(channelName, otherChannel)
                ,(originalChannel) =>
                {
                    return ValueTask.FromResult(newChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.PublishAsync<BasicMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            var result2 = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: otherChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.AreEqual(transmissionResult, result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(transmissionResult, result2);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(newChannel, messages[1].Channel);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestPublishMapWithDefaultFunction()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            
            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            
            var newChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var otherChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddDefaultPublishMap((originalChannel) =>
                {
                    if (Equals(originalChannel, typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name))
                        return ValueTask.FromResult(newChannel);
                    return ValueTask.FromResult(originalChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.PublishAsync<BasicMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            var result2 = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: otherChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.AreEqual(transmissionResult, result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(transmissionResult, result2);
            Assert.AreEqual(newChannel, messages[0].Channel);
            Assert.AreEqual(otherChannel, messages[1].Channel);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestPublishMapWithSingleMapAndWithDefaultFunction()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

            var testMessage = new BasicMessage("testMessage");
            
            List<ServiceMessage> messages = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transmissionResult);
            
            var newChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var otherChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddPublishMap(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name!,newChannel)
                .AddDefaultPublishMap((originalChannel) =>
                {
                    return ValueTask.FromResult(originalChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.PublishAsync<BasicMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            var result2 = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: otherChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.AreEqual(transmissionResult, result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(transmissionResult, result2);
            Assert.AreEqual(newChannel, messages[0].Channel);
            Assert.AreEqual(otherChannel, messages[1].Channel);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestPublishSubscribeMapWithStringToString()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var channels = new List<string>();
            
            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(),
                It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var newChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var mapper = new ChannelMapper()
                .AddPublishSubscriptionMap(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name!, newChannel);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>(
                (msg) => ValueTask.CompletedTask,
                (error) => { });
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(newChannel, channels[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestPublishSubscribeMapWithStringToFunction()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var channels = new List<string>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(),
                It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var newChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var otherChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddPublishSubscriptionMap(
                typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name!,
                (originalChannel) =>
                {
                    if (Equals(originalChannel, typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name))
                        return ValueTask.FromResult(newChannel);
                    return ValueTask.FromResult(originalChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var subscription1 = await contractConnection.SubscribeAsync<BasicMessage>(
                (msg) => ValueTask.CompletedTask,
                (error) => { });
            var subscription2 = await contractConnection.SubscribeAsync<BasicMessage>(
                (msg) => ValueTask.CompletedTask,
                (error) => { },
                channel: otherChannel);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription1);
            Assert.IsNotNull(subscription2);
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(newChannel, channels[0]);
            Assert.AreEqual(otherChannel, channels[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestPublishSubscribeMapWithMatchToFunction()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var channels = new List<string>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(),
                It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var newChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var otherChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddPublishSubscriptionMap(
                (channelName) => Equals(channelName, otherChannel),
                (originalChannel) =>
                {
                    return ValueTask.FromResult(newChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var subscription1 = await contractConnection.SubscribeAsync<BasicMessage>(
                (msg) => ValueTask.CompletedTask,
                (error) => { });
            var subscription2 = await contractConnection.SubscribeAsync<BasicMessage>(
                (msg) => ValueTask.CompletedTask,
                (error) => { },
                channel: otherChannel);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription1);
            Assert.IsNotNull(subscription2);
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.AreEqual(newChannel, channels[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestPublishSubscribeMapWithDefaultFunction()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var channels = new List<string>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(),
                It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var newChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var otherChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddDefaultPublishSubscriptionMap(
                (originalChannel) =>
                {
                    if (Equals(originalChannel, typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name))
                        return ValueTask.FromResult(newChannel);
                    return ValueTask.FromResult(originalChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var subscription1 = await contractConnection.SubscribeAsync<BasicMessage>(
                (msg) => ValueTask.CompletedTask,
                (error) => { });
            var subscription2 = await contractConnection.SubscribeAsync<BasicMessage>(
                (msg) => ValueTask.CompletedTask,
                (error) => { },
                channel: otherChannel);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription1);
            Assert.IsNotNull(subscription2);
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(newChannel, channels[0]);
            Assert.AreEqual(otherChannel, channels[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestPublishSubscribeMapWithSingleMapAndWithDefaultFunction()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var channels = new List<string>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(),
                It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var newChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var otherChannel = $"{typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddPublishSubscriptionMap(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name!, newChannel)
                .AddDefaultPublishSubscriptionMap(
                (originalChannel) =>
                {
                    return ValueTask.FromResult(originalChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var subscription1 = await contractConnection.SubscribeAsync<BasicMessage>(
                (msg) => ValueTask.CompletedTask,
                (error) => { });
            var subscription2 = await contractConnection.SubscribeAsync<BasicMessage>(
                (msg) => ValueTask.CompletedTask,
                (error) => { },
                channel: otherChannel);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription1);
            Assert.IsNotNull(subscription2);
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(newChannel, channels[0]);
            Assert.AreEqual(otherChannel, channels[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestQueryMapWithStringToString()
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

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var newChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var mapper = new ChannelMapper()
                .AddQueryMap(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name!, newChannel);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
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
            Assert.AreEqual(newChannel, messages[0].Channel);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryMapWithStringToFunction()
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

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var newChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var otherChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddQueryMap(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name!, 
                (originalChannel) =>
                {
                    if (Equals(originalChannel, typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name))
                        return ValueTask.FromResult(newChannel);
                    return ValueTask.FromResult(originalChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            var result2 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: otherChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(newChannel, messages[0].Channel);
            Assert.AreEqual(otherChannel, messages[1].Channel);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestQueryMapWithMatchToFunction()
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

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var newChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var otherChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddQueryMap((channelName) => Equals(channelName, otherChannel)
                , (originalChannel) =>
                {
                    return ValueTask.FromResult(newChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            var result2 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: otherChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(newChannel, messages[1].Channel);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestQueryMapWithDefaultFunction()
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

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var newChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var otherChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddDefaultQueryMap((originalChannel) =>
                {
                    if (Equals(originalChannel, typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name))
                        return ValueTask.FromResult(newChannel);
                    return ValueTask.FromResult(originalChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            var result2 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: otherChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(newChannel, messages[0].Channel);
            Assert.AreEqual(otherChannel, messages[1].Channel);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestQueryMapWithSingleMapAndWithDefaultFunction()
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

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var newChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var otherChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddQueryMap(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name!, newChannel)
                .AddDefaultQueryMap((originalChannel) =>
                {
                    return ValueTask.FromResult(originalChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            var result2 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, channel: otherChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(newChannel, messages[0].Channel);
            Assert.AreEqual(otherChannel, messages[1].Channel);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestQuerySubscribeMapWithStringToString()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var channels = new List<string>();
            
            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(),
                It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var newChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var mapper = new ChannelMapper()
                .AddQuerySubscriptionMap(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name!, newChannel);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                throw new NotImplementedException();
            }, (error) => { });
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<string>(channels, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(newChannel, channels[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQuerySubscribeMapWithStringToFunction()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var channels = new List<string>();

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(),
                It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var newChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var otherChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddQuerySubscriptionMap(
                typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name!,
                (originalChannel) =>
                {
                    if (Equals(originalChannel, typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name))
                        return ValueTask.FromResult(newChannel);
                    return ValueTask.FromResult(originalChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var subscription1 = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                throw new NotImplementedException();
            }, (error) => { });
            var subscription2 = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                throw new NotImplementedException();
            }, (error) => { },
            channel: otherChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<string>(channels, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription1);
            Assert.IsNotNull(subscription2);
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(newChannel, channels[0]);
            Assert.AreEqual(otherChannel, channels[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestQuerySubscribeMapWithMatchToFunction()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var channels = new List<string>();

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(),
                It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var newChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var otherChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddQuerySubscriptionMap(
                (channelName) => Equals(channelName, otherChannel),
                (originalChannel) =>
                {
                    return ValueTask.FromResult(newChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var subscription1 = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                throw new NotImplementedException();
            }, (error) => { });
            var subscription2 = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                throw new NotImplementedException();
            }, (error) => { },
            channel: otherChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<string>(channels, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription1);
            Assert.IsNotNull(subscription2);
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.AreEqual(newChannel, channels[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestQuerySubscribeMapWithDefaultFunction()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var channels = new List<string>();

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(),
                It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var newChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var otherChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddDefaultQuerySubscriptionMap(
                (originalChannel) =>
                {
                    if (Equals(originalChannel, typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name))
                        return ValueTask.FromResult(newChannel);
                    return ValueTask.FromResult(originalChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var subscription1 = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                throw new NotImplementedException();
            }, (error) => { });
            var subscription2 = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                throw new NotImplementedException();
            }, (error) => { },
            channel: otherChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<string>(channels, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription1);
            Assert.IsNotNull(subscription2);
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(newChannel, channels[0]);
            Assert.AreEqual(otherChannel, channels[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestQuerySubscribeMapWithSingleMapAndWithDefaultFunction()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var channels = new List<string>();

            var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(),
                It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);

            var newChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-Modded";
            var otherChannel = $"{typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddQuerySubscriptionMap(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name!, newChannel)
                .AddDefaultQuerySubscriptionMap(
                (originalChannel) =>
                {
                    return ValueTask.FromResult(originalChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var subscription1 = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                throw new NotImplementedException();
            }, (error) => { });
            var subscription2 = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                throw new NotImplementedException();
            }, (error) => { },
            channel: otherChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<string>(channels, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription1);
            Assert.IsNotNull(subscription2);
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(newChannel, channels[0]);
            Assert.AreEqual(otherChannel, channels[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        private const string REPLY_CHANNEL_HEADER = "_QueryReplyChannel";

        [TestMethod]
        public async Task TestQueryResponseMapWithStringToString()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);
            var mockSubscription = new Mock<IServiceSubscription>();

            
            List<Action<ReceivedServiceMessage>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<ReceivedServiceMessage>>(messageActions), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    if (message.Header[REPLY_CHANNEL_HEADER]!=null)
                        channels.Add(message.Header[REPLY_CHANNEL_HEADER]!);
                    var resp = new ReceivedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", message.Channel, message.Header, responseData);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });
            
            var responseChannel = "Gretting.Response";

            var newChannel = $"{responseChannel}-Modded";
            var mapper = new ChannelMapper()
                .AddQueryResponseMap(responseChannel, newChannel);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<string>(channels, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(newChannel, channels[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryResponseMapWithStringToFunction()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);
            var mockSubscription = new Mock<IServiceSubscription>();

            
            List<Action<ReceivedServiceMessage>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<ReceivedServiceMessage>>(messageActions), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    if (message.Header[REPLY_CHANNEL_HEADER]!=null)
                        channels.Add(message.Header[REPLY_CHANNEL_HEADER]!);
                    var resp = new ReceivedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", message.Channel, message.Header, responseData);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var responseChannel = "Gretting.Response";

            var newChannel = $"{responseChannel}-Modded";
            var otherChannel = $"{responseChannel}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddQueryResponseMap(responseChannel,
                (originalChannel) =>
                {
                    if (Equals(originalChannel, responseChannel))
                        return ValueTask.FromResult(newChannel);
                    return ValueTask.FromResult(originalChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            var result2 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: otherChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<string>(channels, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(newChannel, channels[0]);
            Assert.AreEqual(otherChannel, channels[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestQueryResponseMapWithMatchToFunction()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);
            var mockSubscription = new Mock<IServiceSubscription>();

            
            List<Action<ReceivedServiceMessage>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<ReceivedServiceMessage>>(messageActions), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    if (message.Header[REPLY_CHANNEL_HEADER]!=null)
                        channels.Add(message.Header[REPLY_CHANNEL_HEADER]!);
                    var resp = new ReceivedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", message.Channel, message.Header, responseData);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var responseChannel = "Gretting.Response";

            var newChannel = $"{responseChannel}-Modded";
            var otherChannel = $"{responseChannel}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddQueryResponseMap((channelName) => Equals(channelName, otherChannel)
                , (originalChannel) =>
                {
                    return ValueTask.FromResult(newChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            var result2 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: otherChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<string>(channels, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(responseChannel, channels[0]);
            Assert.AreEqual(newChannel, channels[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestQueryResponseMapWithDefaultFunction()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);
            var mockSubscription = new Mock<IServiceSubscription>();

            List<Action<ReceivedServiceMessage>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<ReceivedServiceMessage>>(messageActions), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    if (message.Header[REPLY_CHANNEL_HEADER]!=null)
                        channels.Add(message.Header[REPLY_CHANNEL_HEADER]!);
                    var resp = new ReceivedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", message.Channel, message.Header, responseData);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var responseChannel = "Gretting.Response";

            var newChannel = $"{responseChannel}-Modded";
            var otherChannel = $"{responseChannel}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddDefaultQueryResponseMap((originalChannel) =>
                {
                    if (Equals(originalChannel, responseChannel))
                        return ValueTask.FromResult(newChannel);
                    return ValueTask.FromResult(originalChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            var result2 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: otherChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<string>(channels, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(newChannel, channels[0]);
            Assert.AreEqual(otherChannel, channels[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestQueryResponseMapWithSingleMapAndWithDefaultFunction()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);
            var mockSubscription = new Mock<IServiceSubscription>();

            List<Action<ReceivedServiceMessage>> messageActions = [];
            List<string> channels = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<ReceivedServiceMessage>>(messageActions), It.IsAny<Action<Exception>>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    if (message.Header[REPLY_CHANNEL_HEADER]!=null)
                        channels.Add(message.Header[REPLY_CHANNEL_HEADER]!);
                    var resp = new ReceivedServiceMessage(message.ID, "U-BasicResponseMessage-0.0.0.0", message.Channel, message.Header, responseData);
                    foreach (var action in messageActions)
                        action(resp);
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var responseChannel = "Gretting.Response";

            var newChannel = $"{responseChannel}-Modded";
            var otherChannel = $"{responseChannel}-OtherChannel";
            var mapper = new ChannelMapper()
                .AddQueryResponseMap(responseChannel, newChannel)
                .AddDefaultQueryResponseMap((originalChannel) =>
                {
                    return ValueTask.FromResult(originalChannel);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object, channelMapper: mapper);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: responseChannel);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            var result2 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, responseChannel: otherChannel);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<string>(channels, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(newChannel, channels[0]);
            Assert.AreEqual(otherChannel, channels[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
            #endregion
        }
    }
}
