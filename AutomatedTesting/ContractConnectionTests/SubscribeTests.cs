﻿using AutomatedTesting.Messages;
using Moq;
using MQContract.Attributes;
using MQContract;
using System.Diagnostics;
using MQContract.Interfaces.Service;
using MQContract.Interfaces;
using System.Reflection;

namespace AutomatedTesting.ContractConnectionTests
{
    [TestClass]
    public class SubscribeTests
    {
        [TestMethod]
        public async Task TestSubscribeAsyncWithNoExtendedAspects()
        {
            #region Arrange
            var transmissionResult = new Mock<ITransmissionResult>();   
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Action<IRecievedServiceMessage>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<IRecievedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<IRecievedServiceMessage>>(actions), Capture.In<Action<Exception>>(errorActions), Capture.In<string>(channels),
                Capture.In<string>(groups), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(serviceSubscription.Object));
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns((IServiceMessage message, TimeSpan timeout, IServiceChannelOptions options, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceRecievedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return Task.FromResult(transmissionResult.Object);
                });

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IMessage<BasicMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>((msg) => {
                messages.Add(msg);
                return Task.CompletedTask;
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(message);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            await Task.Delay(TimeSpan.FromSeconds(5));
            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsFalse(string.IsNullOrWhiteSpace(groups[0]));
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(serviceMessages[0].RecievedTimestamp, messages[0].RecievedTimestamp);
            Assert.AreEqual(message, messages[0].Message);
            Assert.AreEqual(exception, exceptions[0]);
            System.Diagnostics.Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].RecievedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithSpecificChannel()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var channels = new List<string>();
            var channelName = "TestSubscribeAsyncWithSpecificChannel";

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), Capture.In<string>(channels),
                It.IsAny<string>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(serviceSubscription.Object));
            
            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var subscription1 = await contractConnection.SubscribeAsync<BasicMessage>((msg) => Task.CompletedTask, (error) => { },channel:channelName);
            var subscription2 = await contractConnection.SubscribeAsync<NoChannelMessage>((msg) => Task.CompletedTask, (error) => { }, channel: channelName);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription1);
            Assert.IsNotNull(subscription2);
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(channelName, channels[0]);
            Assert.AreEqual(channelName, channels[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithSpecificGroup()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var groups = new List<string>();
            var groupName = "TestSubscribeAsyncWithSpecificGroup";

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                Capture.In<string>(groups), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(serviceSubscription.Object));

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var subscription1 = await contractConnection.SubscribeAsync<BasicMessage>((msg) => Task.CompletedTask, (error) => { }, group:groupName);
            var subscription2 = await contractConnection.SubscribeAsync<BasicMessage>((msg) => Task.CompletedTask, (error) => { });
            #endregion

            #region Assert
            Assert.IsNotNull(subscription1);
            Assert.IsNotNull(subscription2);
            Assert.AreEqual(2, groups.Count);
            Assert.AreEqual(groupName, groups[0]);
            Assert.AreNotEqual(groupName, groups[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithServiceChannelOptions()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var serviceChannelOptions = new TestServiceChannelOptions("TestSubscribeAsyncWithServiceChannelOptions");
            List<IServiceChannelOptions> options = [];

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), Capture.In<IServiceChannelOptions>(options), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(serviceSubscription.Object));

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>((msg) => Task.CompletedTask, (error) => { }, options:serviceChannelOptions);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            Assert.AreEqual(1, options.Count);
            Assert.IsInstanceOfType<TestServiceChannelOptions>(options[0]);
            Assert.AreEqual(serviceChannelOptions, options[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncNoMessageChannelThrowsError()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(serviceSubscription.Object));

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var exception = await Assert.ThrowsExceptionAsync<MessageChannelNullException>(() => contractConnection.SubscribeAsync<NoChannelMessage>((msg) => Task.CompletedTask, (error) => { }));
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual("message must have a channel value (Parameter 'channel')", exception.Message);
            Assert.AreEqual("channel", exception.ParamName);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncReturnFailedSubscription()
        {
            #region Arrange
            var serviceConnection = new Mock<IMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IServiceSubscription?>(null));

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var exception = await Assert.ThrowsExceptionAsync<SubscriptionFailedException>(()=>contractConnection.SubscribeAsync<BasicMessage>(msg => Task.CompletedTask, err => { }));
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncCleanup()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            serviceSubscription.Setup(x => x.EndAsync())
                .Returns(Task.CompletedTask);

            var serviceConnection = new Mock<IMessageServiceConnection>();

            serviceConnection.Setup(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(serviceSubscription.Object));

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>(msg => Task.CompletedTask, err => { });
            await subscription.EndAsync();
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithSynchronousActions()
        {
            #region Arrange
            var transmissionResult = new Mock<ITransmissionResult>();
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Action<IRecievedServiceMessage>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<IRecievedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<IRecievedServiceMessage>>(actions), Capture.In<Action<Exception>>(errorActions), Capture.In<string>(channels),
                Capture.In<string>(groups), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(serviceSubscription.Object));
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns((IServiceMessage message, TimeSpan timeout, IServiceChannelOptions options, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceRecievedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return Task.FromResult(transmissionResult.Object);
                });

            var message1 = new BasicMessage("TestSubscribeAsyncWithSynchronousActions1");
            var message2 = new BasicMessage("TestSubscribeAsyncWithSynchronousActions2");
            var exception = new NullReferenceException("TestSubscribeAsyncWithSynchronousActions");

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IMessage<BasicMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>((msg) => {
                messages.Add(msg);
                return Task.CompletedTask;
            }, (error) => exceptions.Add(error),synchronous:true);
            var stopwatch = Stopwatch.StartNew();
            var result1 = await contractConnection.PublishAsync<BasicMessage>(message1);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();
            var result2 = await contractConnection.PublishAsync<BasicMessage>(message2);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            await Task.Delay(TimeSpan.FromSeconds(1));
            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(2, serviceMessages.Count);
            Assert.AreEqual(2, messages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsFalse(string.IsNullOrWhiteSpace(groups[0]));
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(serviceMessages[0].RecievedTimestamp, messages[0].RecievedTimestamp);
            Assert.AreEqual(message1, messages[0].Message);
            Assert.AreEqual(message2, messages[1].Message);
            Assert.AreEqual(exception, exceptions[0]);
            System.Diagnostics.Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].RecievedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithErrorTriggeringInOurAction()
        {
            #region Arrange
            var transmissionResult = new Mock<ITransmissionResult>();
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Action<IRecievedServiceMessage>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<IRecievedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<IRecievedServiceMessage>>(actions), Capture.In<Action<Exception>>(errorActions), Capture.In<string>(channels),
                Capture.In<string>(groups), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(serviceSubscription.Object));
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns((IServiceMessage message, TimeSpan timeout, IServiceChannelOptions options, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceRecievedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return Task.FromResult(transmissionResult.Object);
                });

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IMessage<BasicMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>((msg) => {
                throw exception;
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(message);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            await Task.Delay(TimeSpan.FromSeconds(1));
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(0, messages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsFalse(string.IsNullOrWhiteSpace(groups[0]));
            Assert.AreEqual(exception, exceptions[0]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithCorruptMetaDataHeaderException()
        {
            #region Arrange
            var transmissionResult = new Mock<ITransmissionResult>();
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Action<IRecievedServiceMessage>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<IRecievedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<IRecievedServiceMessage>>(actions), Capture.In<Action<Exception>>(errorActions), Capture.In<string>(channels),
                Capture.In<string>(groups), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(serviceSubscription.Object));
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns((IServiceMessage message, TimeSpan timeout, IServiceChannelOptions options, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceRecievedServiceMessage(message,$"{message.MessageTypeID}:XXXX");
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return Task.FromResult(transmissionResult.Object);
                });

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IMessage<BasicMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>((msg) => { return Task.CompletedTask; }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(message);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            await Task.Delay(TimeSpan.FromSeconds(1));
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(0, messages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsFalse(string.IsNullOrWhiteSpace(groups[0]));
            Assert.IsInstanceOfType<InvalidDataException>(exceptions[0]);
            Assert.AreEqual("MetaData is not valid", exceptions[0].Message);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithDisposal()
        {
            #region Arrange
            var transmissionResult = new Mock<ITransmissionResult>();
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Action<IRecievedServiceMessage>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<IRecievedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<IRecievedServiceMessage>>(actions), Capture.In<Action<Exception>>(errorActions), Capture.In<string>(channels),
                Capture.In<string>(groups), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(serviceSubscription.Object));
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns((IServiceMessage message, TimeSpan timeout, IServiceChannelOptions options, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceRecievedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return Task.FromResult(transmissionResult.Object);
                });

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IMessage<BasicMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>((msg) => {
                messages.Add(msg);
                return Task.CompletedTask;
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(message);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            await Task.Delay(TimeSpan.FromSeconds(1));
            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsFalse(string.IsNullOrWhiteSpace(groups[0]));
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(serviceMessages[0].RecievedTimestamp, messages[0].RecievedTimestamp);
            Assert.AreEqual(message, messages[0].Message);
            Assert.AreEqual(exception, exceptions[0]);
            System.Diagnostics.Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].RecievedTimestamp).TotalMilliseconds}ms");
            Exception? disposeError = null;
            try
            {
                subscription.Dispose();
            }catch(Exception e)
            {
                disposeError=e;
            }
            Assert.IsNull(disposeError);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithSingleConversion()
        {
            #region Arrange
            var transmissionResult = new Mock<ITransmissionResult>();
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Action<IRecievedServiceMessage>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<IRecievedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<IRecievedServiceMessage>>(actions), Capture.In<Action<Exception>>(errorActions), Capture.In<string>(channels),
                Capture.In<string>(groups), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(serviceSubscription.Object));
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns((IServiceMessage message, TimeSpan timeout, IServiceChannelOptions options, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceRecievedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return Task.FromResult(transmissionResult.Object);
                });

            var message = new BasicMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IMessage<NamedAndVersionedMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<NamedAndVersionedMessage>((msg) => {
                messages.Add(msg);
                return Task.CompletedTask;
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicMessage>(message,channel:typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            await Task.Delay(TimeSpan.FromSeconds(1));
            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsFalse(string.IsNullOrWhiteSpace(groups[0]));
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(serviceMessages[0].RecievedTimestamp, messages[0].RecievedTimestamp);
            Assert.AreEqual(message.Name, messages[0].Message.TestName);
            Assert.AreEqual(exception, exceptions[0]);
            System.Diagnostics.Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].RecievedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithMultipleStepConversion()
        {
            #region Arrange
            var transmissionResult = new Mock<ITransmissionResult>();
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Action<IRecievedServiceMessage>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<IRecievedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<IRecievedServiceMessage>>(actions), Capture.In<Action<Exception>>(errorActions), Capture.In<string>(channels),
                Capture.In<string>(groups), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(serviceSubscription.Object));
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns((IServiceMessage message, TimeSpan timeout, IServiceChannelOptions options, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceRecievedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return Task.FromResult(transmissionResult.Object);
                });

            var message = new NoChannelMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IMessage<NamedAndVersionedMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<NamedAndVersionedMessage>((msg) => {
                messages.Add(msg);
                return Task.CompletedTask;
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<NoChannelMessage>(message, channel: typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            await Task.Delay(TimeSpan.FromSeconds(1));
            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsFalse(string.IsNullOrWhiteSpace(groups[0]));
            Assert.AreEqual(serviceMessages[0].ID, messages[0].ID);
            Assert.AreEqual(serviceMessages[0].Header.Keys.Count(), messages[0].Headers.Keys.Count());
            Assert.AreEqual(serviceMessages[0].RecievedTimestamp, messages[0].RecievedTimestamp);
            Assert.AreEqual(message.TestName, messages[0].Message.TestName);
            Assert.AreEqual(exception, exceptions[0]);
            System.Diagnostics.Trace.WriteLine($"Time to process message {messages[0].ProcessedTimestamp.Subtract(messages[0].RecievedTimestamp).TotalMilliseconds}ms");
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeAsyncWithNoConversionPath()
        {
            #region Arrange
            var transmissionResult = new Mock<ITransmissionResult>();
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceConnection = new Mock<IMessageServiceConnection>();

            var actions = new List<Action<IRecievedServiceMessage>>();
            var errorActions = new List<Action<Exception>>();
            var channels = new List<string>();
            var groups = new List<string>();
            var serviceMessages = new List<IRecievedServiceMessage>();

            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<IRecievedServiceMessage>>(actions), Capture.In<Action<Exception>>(errorActions), Capture.In<string>(channels),
                Capture.In<string>(groups), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(serviceSubscription.Object));
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns((IServiceMessage message, TimeSpan timeout, IServiceChannelOptions options, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceRecievedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return Task.FromResult(transmissionResult.Object);
                });

            var message = new BasicQueryMessage("TestSubscribeAsyncWithNoExtendedAspects");
            var exception = new NullReferenceException("TestSubscribeAsyncWithNoExtendedAspects");

            var contractConnection = new ContractConnection(serviceConnection.Object);
            #endregion

            #region Act
            var messages = new List<IMessage<NamedAndVersionedMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeAsync<NamedAndVersionedMessage>((msg) => {
                messages.Add(msg);
                return Task.CompletedTask;
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.PublishAsync<BasicQueryMessage>(message, channel: typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            await Task.Delay(TimeSpan.FromSeconds(1));
            foreach (var act in errorActions)
                act(exception);
            #endregion

            #region Assert
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, channels.Count);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, serviceMessages.Count);
            Assert.AreEqual(0, messages.Count);
            Assert.AreEqual(1, errorActions.Count);
            Assert.AreEqual(2, exceptions.Count);
            Assert.AreEqual(typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsFalse(string.IsNullOrWhiteSpace(groups[0]));
            Assert.IsInstanceOfType<InvalidCastException>(exceptions[0]);
            Assert.AreEqual(exception, exceptions[1]);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<IRecievedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<IServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }
    }
}
