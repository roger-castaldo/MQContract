using AutomatedTesting.Messages;
using Moq;
using MQContract.Interfaces.Service;
using MQContract;
using MQContract.Interfaces;

namespace AutomatedTesting.ContractConnectionTests
{
    [TestClass]
    public class InternalMetricTests
    {
        [TestMethod]
        public void TestInternalMetricInitialization()
        {
            #region Arrange
            var channel = "TestInternalMetricsChannel";

            var serviceConnection = new Mock<IMessageServiceConnection>();
            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .AddMetrics(null, true);
            #endregion

            #region Act
            IContractMetric?[] sentMetrics = [
                contractConnection.GetSnapshot(true),
                contractConnection.GetSnapshot(typeof(BasicMessage),true),
                contractConnection.GetSnapshot<BasicMessage>(true),
                contractConnection.GetSnapshot(channel,true)
            ];
            IContractMetric?[] receivedMetrics = [
                contractConnection.GetSnapshot(false),
                contractConnection.GetSnapshot(typeof(BasicMessage),false),
                contractConnection.GetSnapshot<BasicMessage>(false),
                contractConnection.GetSnapshot(channel,false)
            ];
            #endregion

            #region Assert
            Assert.IsNotNull(sentMetrics[0]);
            Assert.IsNotNull(receivedMetrics[0]);
            Assert.IsFalse(sentMetrics.Skip(1).Any(m => m!=null));
            Assert.AreEqual<ulong?>(ulong.MaxValue, sentMetrics[0]?.MessageBytesMin);
            Assert.AreEqual<ulong?>(0, sentMetrics[0]?.MessageBytes);
            Assert.AreEqual<ulong?>(ulong.MinValue, sentMetrics[0]?.MessageBytesMax);
            Assert.AreEqual<ulong?>(0, sentMetrics[0]?.MessageBytesAverage);
            Assert.AreEqual<ulong?>(0, sentMetrics[0]?.Messages);
            Assert.AreEqual<TimeSpan?>(TimeSpan.MaxValue, sentMetrics[0]?.MessageConversionMin);
            Assert.AreEqual<TimeSpan?>(TimeSpan.MinValue, sentMetrics[0]?.MessageConversionMax);
            Assert.AreEqual<TimeSpan?>(TimeSpan.Zero, sentMetrics[0]?.MessageConversionAverage);
            Assert.AreEqual<TimeSpan?>(TimeSpan.Zero, sentMetrics[0]?.MessageConversionDuration);

            Assert.IsFalse(receivedMetrics.Skip(1).Any(m => m!=null));
            Assert.AreEqual<ulong?>(ulong.MaxValue, receivedMetrics[0]?.MessageBytesMin);
            Assert.AreEqual<ulong?>(0, receivedMetrics[0]?.MessageBytes);
            Assert.AreEqual<ulong?>(ulong.MinValue, receivedMetrics[0]?.MessageBytesMax);
            Assert.AreEqual<ulong?>(0, receivedMetrics[0]?.MessageBytesAverage);
            Assert.AreEqual<ulong?>(0, receivedMetrics[0]?.Messages);
            Assert.AreEqual<TimeSpan?>(TimeSpan.MaxValue, receivedMetrics[0]?.MessageConversionMin);
            Assert.AreEqual<TimeSpan?>(TimeSpan.MinValue, receivedMetrics[0]?.MessageConversionMax);
            Assert.AreEqual<TimeSpan?>(TimeSpan.Zero, receivedMetrics[0]?.MessageConversionAverage);
            Assert.AreEqual<TimeSpan?>(TimeSpan.Zero, receivedMetrics[0]?.MessageConversionDuration);
            #endregion

            #region Verify
            #endregion
        }

        [TestMethod]
        public void TestInternalMetricGetSnapshotsWithoutEnabling()
        {
            #region Arrange
            var channel = "TestInternalMetricsChannel";

            var serviceConnection = new Mock<IMessageServiceConnection>();
            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .AddMetrics(null, false);
            #endregion

            #region Act
            IContractMetric?[] sentMetrics = [
                contractConnection.GetSnapshot(true),
                contractConnection.GetSnapshot(typeof(BasicMessage),true),
                contractConnection.GetSnapshot<BasicMessage>(true),
                contractConnection.GetSnapshot(channel,true)
            ];
            IContractMetric?[] receivedMetrics = [
                contractConnection.GetSnapshot(false),
                contractConnection.GetSnapshot(typeof(BasicMessage),false),
                contractConnection.GetSnapshot<BasicMessage>(false),
                contractConnection.GetSnapshot(channel,false)
            ];
            #endregion

            #region Assert
            Assert.IsTrue(Array.TrueForAll(sentMetrics,m => m==null));
            Assert.IsTrue(Array.TrueForAll(receivedMetrics,m => m==null));
            #endregion

            #region Verify
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncSubscribeInternalMetrics()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();

            var testMessage = new BasicMessage("testMessage");
            var channel = "TestInternalMetricsChannel";

            List<ServiceMessage> messages = [];
            var actions = new List<Action<ReceivedServiceMessage>>();

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In<Action<ReceivedServiceMessage>>(actions), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(Capture.In<ServiceMessage>(messages), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .AddMetrics(null,true);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeAsync<BasicMessage>(
                (msg) => ValueTask.CompletedTask, 
                (error) => { },
                channel:channel);
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage,channel:channel);
            _ = await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1));
            await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(true);
            IContractMetric?[] sentMetrics = [
                contractConnection.GetSnapshot(true),
                contractConnection.GetSnapshot(typeof(BasicMessage),true),
                contractConnection.GetSnapshot<BasicMessage>(true),
                contractConnection.GetSnapshot(channel,true)
            ];
            IContractMetric?[] receivedMetrics = [
                contractConnection.GetSnapshot(false),
                contractConnection.GetSnapshot(typeof(BasicMessage),false),
                contractConnection.GetSnapshot<BasicMessage>(false),
                contractConnection.GetSnapshot(channel,false)
            ];
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.IsFalse(Array.Exists(sentMetrics,(m => m==null)));
            Assert.AreEqual<ulong?>(1, sentMetrics[0]?.Messages);
            Assert.IsTrue(Array.TrueForAll(sentMetrics,(m =>
                Equals(sentMetrics[0]?.MessageBytesMin,m?.MessageBytesMin) &&
                Equals(sentMetrics[0]?.MessageBytes, m?.MessageBytes) &&
                Equals(sentMetrics[0]?.MessageBytesMax, m?.MessageBytesMax) &&
                Equals(sentMetrics[0]?.MessageBytesAverage, m?.MessageBytesAverage) &&
                Equals(sentMetrics[0]?.Messages, m?.Messages) &&
                Equals(sentMetrics[0]?.MessageConversionMin, m?.MessageConversionMin) &&
                Equals(sentMetrics[0]?.MessageConversionMax, m?.MessageConversionMax) &&
                Equals(sentMetrics[0]?.MessageConversionAverage, m?.MessageConversionAverage) &&
                Equals(sentMetrics[0]?.MessageConversionDuration, m?.MessageConversionDuration)
            )));

            Assert.IsFalse(Array.Exists(receivedMetrics, (m => m==null)));
            Assert.AreEqual<ulong?>(1, receivedMetrics[0]?.Messages);
            Assert.IsTrue(Array.TrueForAll(receivedMetrics,(m =>
                Equals(receivedMetrics[0]?.MessageBytesMin, m?.MessageBytesMin) &&
                Equals(receivedMetrics[0]?.MessageBytes, m?.MessageBytes) &&
                Equals(receivedMetrics[0]?.MessageBytesMax, m?.MessageBytesMax) &&
                Equals(receivedMetrics[0]?.MessageBytesAverage, m?.MessageBytesAverage) &&
                Equals(receivedMetrics[0]?.Messages, m?.Messages) &&
                Equals(receivedMetrics[0]?.MessageConversionMin, m?.MessageConversionMin) &&
                Equals(receivedMetrics[0]?.MessageConversionMax, m?.MessageConversionMax) &&
                Equals(receivedMetrics[0]?.MessageConversionAverage, m?.MessageConversionAverage) &&
                Equals(receivedMetrics[0]?.MessageConversionDuration, m?.MessageConversionDuration)
            )));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithNoExtendedAspects()
        {
            #region Arrange
            var serviceSubscription = new Mock<IServiceSubscription>();

            var receivedActions = new List<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>();

            var serviceConnection = new Mock<IQueryableMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeQueryAsync(
                Capture.In<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(receivedActions),
                It.IsAny<Action<Exception>>(),
                It.IsAny<string>(),
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(async (ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message);
                    var result = await receivedActions[0](rmessage);
                    return Helper.ProduceQueryResult(result);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object)
                .AddMetrics(null, true);

            var message = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var channel = "TestQueryMetricChannel";
            #endregion

            #region Act
            _ = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                return ValueTask.FromResult(new QueryResponseMessage<BasicResponseMessage>(responseMessage, null));
            }, (error) => { },
            channel:channel);
            _ = await contractConnection.QueryAsync<BasicQueryMessage>(message,channel:channel);
            await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(true);
            IContractMetric?[] querySentMetrics = [
                contractConnection.GetSnapshot(typeof(BasicQueryMessage),true),
                contractConnection.GetSnapshot<BasicQueryMessage>(true)
            ];
            IContractMetric?[] queryReceivedMetrics = [
                contractConnection.GetSnapshot(typeof(BasicQueryMessage),false),
                contractConnection.GetSnapshot<BasicQueryMessage>(false)
            ];
            IContractMetric?[] responseSentMetrics = [
                contractConnection.GetSnapshot(typeof(BasicResponseMessage),true),
                contractConnection.GetSnapshot<BasicResponseMessage>(true)
            ];
            IContractMetric?[] responseReceivedMetrics = [
                contractConnection.GetSnapshot(typeof(BasicResponseMessage),false),
                contractConnection.GetSnapshot<BasicResponseMessage>(false)
            ];
            IContractMetric? globalSentMetrics = contractConnection.GetSnapshot(true);
            IContractMetric? globalReceivedMetrics = contractConnection.GetSnapshot(false);
            IContractMetric? channelSentMetrics = contractConnection.GetSnapshot(channel, true);
            IContractMetric? channelRecievedMetrics = contractConnection.GetSnapshot(channel, false);
            #endregion

            #region Assert
            Assert.IsNotNull(globalSentMetrics);
            Assert.AreEqual<ulong?>(2, globalSentMetrics.Messages);
            Assert.IsNotNull(globalReceivedMetrics);
            Assert.AreEqual<ulong?>(2, globalReceivedMetrics.Messages);
            Assert.IsNotNull(channelSentMetrics);
            Assert.AreEqual<ulong?>(1, channelSentMetrics.Messages);
            Assert.IsNotNull(channelRecievedMetrics);
            Assert.AreEqual<ulong?>(1, channelRecievedMetrics.Messages);
            Assert.IsTrue(Array.TrueForAll(querySentMetrics, (q) => q!=null));
            Assert.IsTrue(Array.TrueForAll(queryReceivedMetrics, (q) => q!=null));
            Assert.IsTrue(Array.TrueForAll(responseSentMetrics, (q) => q!=null));
            Assert.IsTrue(Array.TrueForAll(responseReceivedMetrics, (q) => q!=null));
            Assert.AreEqual<ulong?>(1, querySentMetrics[0]?.Messages);
            Assert.IsTrue(Array.TrueForAll(querySentMetrics, (m =>
                Equals(querySentMetrics[0]?.MessageBytesMin, m?.MessageBytesMin) &&
                Equals(querySentMetrics[0]?.MessageBytes, m?.MessageBytes) &&
                Equals(querySentMetrics[0]?.MessageBytesMax, m?.MessageBytesMax) &&
                Equals(querySentMetrics[0]?.MessageBytesAverage, m?.MessageBytesAverage) &&
                Equals(querySentMetrics[0]?.Messages, m?.Messages) &&
                Equals(querySentMetrics[0]?.MessageConversionMin, m?.MessageConversionMin) &&
                Equals(querySentMetrics[0]?.MessageConversionMax, m?.MessageConversionMax) &&
                Equals(querySentMetrics[0]?.MessageConversionAverage, m?.MessageConversionAverage) &&
                Equals(querySentMetrics[0]?.MessageConversionDuration, m?.MessageConversionDuration)
            )));
            Assert.AreEqual<ulong?>(1, queryReceivedMetrics[0]?.Messages);
            Assert.IsTrue(Array.TrueForAll(queryReceivedMetrics, (m =>
                Equals(queryReceivedMetrics[0]?.MessageBytesMin, m?.MessageBytesMin) &&
                Equals(queryReceivedMetrics[0]?.MessageBytes, m?.MessageBytes) &&
                Equals(queryReceivedMetrics[0]?.MessageBytesMax, m?.MessageBytesMax) &&
                Equals(queryReceivedMetrics[0]?.MessageBytesAverage, m?.MessageBytesAverage) &&
                Equals(queryReceivedMetrics[0]?.Messages, m?.Messages) &&
                Equals(queryReceivedMetrics[0]?.MessageConversionMin, m?.MessageConversionMin) &&
                Equals(queryReceivedMetrics[0]?.MessageConversionMax, m?.MessageConversionMax) &&
                Equals(queryReceivedMetrics[0]?.MessageConversionAverage, m?.MessageConversionAverage) &&
                Equals(queryReceivedMetrics[0]?.MessageConversionDuration, m?.MessageConversionDuration)
            )));
            Assert.AreEqual<ulong?>(1, responseSentMetrics[0]?.Messages);
            Assert.IsTrue(Array.TrueForAll(responseSentMetrics, (m =>
                Equals(responseSentMetrics[0]?.MessageBytesMin, m?.MessageBytesMin) &&
                Equals(responseSentMetrics[0]?.MessageBytes, m?.MessageBytes) &&
                Equals(responseSentMetrics[0]?.MessageBytesMax, m?.MessageBytesMax) &&
                Equals(responseSentMetrics[0]?.MessageBytesAverage, m?.MessageBytesAverage) &&
                Equals(responseSentMetrics[0]?.Messages, m?.Messages) &&
                Equals(responseSentMetrics[0]?.MessageConversionMin, m?.MessageConversionMin) &&
                Equals(responseSentMetrics[0]?.MessageConversionMax, m?.MessageConversionMax) &&
                Equals(responseSentMetrics[0]?.MessageConversionAverage, m?.MessageConversionAverage) &&
                Equals(responseSentMetrics[0]?.MessageConversionDuration, m?.MessageConversionDuration)
            )));
            Assert.AreEqual<ulong?>(1, responseReceivedMetrics[0]?.Messages);
            Assert.IsTrue(Array.TrueForAll(responseReceivedMetrics, (m =>
                Equals(responseReceivedMetrics[0]?.MessageBytesMin, m?.MessageBytesMin) &&
                Equals(responseReceivedMetrics[0]?.MessageBytes, m?.MessageBytes) &&
                Equals(responseReceivedMetrics[0]?.MessageBytesMax, m?.MessageBytesMax) &&
                Equals(responseReceivedMetrics[0]?.MessageBytesAverage, m?.MessageBytesAverage) &&
                Equals(responseReceivedMetrics[0]?.Messages, m?.Messages) &&
                Equals(responseReceivedMetrics[0]?.MessageConversionMin, m?.MessageConversionMin) &&
                Equals(responseReceivedMetrics[0]?.MessageConversionMax, m?.MessageConversionMax) &&
                Equals(responseReceivedMetrics[0]?.MessageConversionAverage, m?.MessageConversionAverage) &&
                Equals(responseReceivedMetrics[0]?.MessageConversionDuration, m?.MessageConversionDuration)
            )));
            Assert.AreEqual(querySentMetrics[0]?.MessageBytes, queryReceivedMetrics[0]?.MessageBytes);
            Assert.AreEqual(responseSentMetrics[0]?.MessageBytes, responseReceivedMetrics[0]?.MessageBytes);
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }
    }
}
