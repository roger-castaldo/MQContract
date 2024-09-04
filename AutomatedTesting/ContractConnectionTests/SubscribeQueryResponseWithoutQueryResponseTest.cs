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
            List<Action<RecievedServiceMessage>> messageActions = [];
            var defaultTimeout = TimeSpan.FromMinutes(1);

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<RecievedServiceMessage>>(messageActions), It.IsAny<Action<Exception>>(),
                Capture.In<string>(channels), Capture.In<string>(groups),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubObject);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, IServiceChannelOptions options, CancellationToken cancellationToken) =>
                {
                    messages.Add(message);
                    foreach (var action in messageActions)
                        action(new RecievedServiceMessage(message.ID,message.MessageTypeID,message.Channel,message.Header,message.Data));
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });
            serviceConnection.Setup(x => x.DefaultTimout)
                .Returns(defaultTimeout);

            var contractConnection = new ContractConnection(serviceConnection.Object);

            var message = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            #endregion

            #region Act
            var recievedMessages = new List<IRecievedMessage<BasicQueryMessage>>();
            var exceptions = new List<Exception>();
            var subscription = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) => {
                recievedMessages.Add(msg);
                return ValueTask.FromResult(new QueryResponseMessage<BasicResponseMessage>(responseMessage, null));
            }, (error) => exceptions.Add(error));
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage>(message);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");

            await subscription.EndAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<IRecievedMessage<BasicQueryMessage>>(recievedMessages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 2, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(subscription);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, channels.Count);
            Assert.AreEqual(2, groups.Count);
            Assert.AreEqual(2, messages.Count);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, channels[0]);
            Assert.IsFalse(string.IsNullOrWhiteSpace(groups[0]));
            Assert.AreEqual(recievedMessages[0].ID, messages[0].ID);
            Assert.AreEqual(0,recievedMessages[0].Headers.Keys.Count());
            Assert.AreEqual(3, messages[0].Header.Keys.Count());
            Assert.AreEqual(message, recievedMessages[0].Message);
            Assert.IsFalse(result.IsError);
            Assert.IsNull(result.Error);
            Assert.AreEqual(result.Result, responseMessage);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<RecievedServiceMessage>>(), It.IsAny<Action<Exception>>(),
               It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IServiceChannelOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            serviceSubscription.Verify(x => x.EndAsync(), Times.Exactly(2));
            #endregion
        }
    }
}
