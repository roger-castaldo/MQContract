using AutomatedTesting.Messages;
using Moq;
using MQContract.Attributes;
using MQContract.Interfaces.Service;
using MQContract.Interfaces;
using MQContract;
using System.Diagnostics;
using System.Reflection;

namespace AutomatedTesting.ContractConnectionTests
{
    [TestClass]
    public class SubscribeQueryResponseWithoutQueryResponseTest
    {
        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithNoExtendedAspects()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();
            var serviceSubObject = serviceSubscription.Object;

            var channels = new List<string>();
            var groups = new List<string>();
            List<ServiceMessage> messages = [];
            List<Action<ReceivedServiceMessage>> messageActions = [];

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<ReceivedServiceMessage>>(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels), Capture.In<string>(groups), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubObject);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    messages.Add(message);
                    foreach (var action in messageActions)
                        action(new ReceivedServiceMessage(message.ID,message.MessageTypeID,message.Channel,message.Header,message.Data));
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);

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
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage>(message);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            await subscription.EndAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<IReceivedMessage<BasicQueryMessage>>(receivedMessages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(2, groups.Count);
            Assert.AreEqual(2, messages.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsNull(groups[0]);
            Assert.IsNull(groups[1]);
            Assert.AreEqual(receivedMessages[0].ID, messages[0].ID);
            Assert.AreEqual(0,receivedMessages[0].Headers.Keys.Count());
            Assert.AreEqual(3, messages[0].Header.Keys.Count());
            Assert.AreEqual(message, receivedMessages[0].Message);
            Assert.IsFalse(result.IsError);
            Assert.IsNull(result.Error);
            Assert.AreEqual(result.Result, responseMessage);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
            #endregion
        }
    }
}
