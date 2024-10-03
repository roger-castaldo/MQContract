using AutomatedTesting.Messages;
using Moq;
using MQContract.Attributes;
using MQContract.Interfaces.Service;
using MQContract;
using System.Diagnostics;
using System.Text.Json;
using System.Reflection;

namespace AutomatedTesting.ContractConnectionTests
{
    [TestClass]
    public class QueryInboxTests
    {
        [TestMethod]
        public async Task TestQueryAsyncWithNoExtendedAspects()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var responseMessage = new BasicResponseMessage("testResponse");
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<BasicResponseMessage>(ms, responseMessage);
            var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

            var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);

            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromMinutes(1);

            List<Action<ReceivedInboxServiceMessage>> receivedActions = [];
            List<ServiceMessage> messages = [];
            List<Guid> messageIDs = [];
            var acknowledgeCount = 0;


            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x=>x.EstablishInboxSubscriptionAsync(Capture.In<Action<ReceivedInboxServiceMessage>>(receivedActions),It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(Capture.In<ServiceMessage>(messages), Capture.In<Guid>(messageIDs), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message,Guid messageID, CancellationToken cancellationToken) =>
                {
                    foreach (var action in receivedActions)
                        action(new(
                            queryResult.ID,
                            queryResult.MessageTypeID,
                            message.Channel,
                            queryResult.Header,
                            messageID,
                            queryResult.Data,
                            () => { 
                                acknowledgeCount++;
                                return ValueTask.CompletedTask;
                            }
                        ));
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var result = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage);
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1)));
            Assert.IsNotNull(result);
            Assert.AreEqual(queryResult.ID, result.ID);
            Assert.IsNull(result.Error);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(BasicQueryMessage).GetCustomAttribute<MessageChannelAttribute>(false)?.Name, messages[0].Channel);
            Assert.AreEqual(1, messageIDs.Count);
            Assert.AreEqual(0, messages[0].Header.Keys.Count());
            Assert.AreEqual("U-BasicQueryMessage-0.0.0.0", messages[0].MessageTypeID);
            Assert.IsTrue(messages[0].Data.Length>0);
            Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray())));
            Assert.AreEqual(responseMessage, result.Result);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithErrorInPublish()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");
            var errorMessage = "Unable to transmit";
            
            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromMinutes(1);

            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, Guid messageID, CancellationToken cancellationToken) =>
                {
                    return ValueTask.FromResult(new TransmissionResult(message.ID,errorMessage));
                });
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var exception = await Assert.ThrowsExceptionAsync<QuerySubmissionFailedException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage));
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(errorMessage, exception.Message);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryAsyncWithTimeout()
        {
            #region Arrange
            var testMessage = new BasicQueryMessage("testMessage");

            var mockSubscription = new Mock<IServiceSubscription>();

            var defaultTimeout = TimeSpan.FromSeconds(5);

            var serviceConnection = new Mock<IInboxQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, Guid messageID, CancellationToken cancellationToken) =>
                {
                    return ValueTask.FromResult(new TransmissionResult(message.ID));
                });
            serviceConnection.Setup(x => x.DefaultTimeout)
                .Returns(defaultTimeout);

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var stopwatch = Stopwatch.StartNew();
            var exception = await Assert.ThrowsExceptionAsync<QueryTimeoutException>(async () => await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage));
            stopwatch.Stop();
            System.Diagnostics.Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
            await contractConnection.CloseAsync();
            #endregion

            #region Assert
            Assert.IsNotNull(exception);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.EstablishInboxSubscriptionAsync(It.IsAny<Action<ReceivedInboxServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSubscription.Verify(x => x.EndAsync(), Times.Once);
            #endregion
        }
    }
}
