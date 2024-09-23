using AutomatedTesting.Messages;
using Moq;
using MQContract.Interfaces.Service;
using MQContract;
using MQContract.Attributes;
using System.Reflection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using System.Diagnostics.Metrics;

namespace AutomatedTesting.ContractConnectionTests
{
    [TestClass]
    public class SystemMetricTests
    {
        private const string MeterName = "mqcontract";
        private static (MetricCollector<long> sent,MetricCollector<long> sentBytes,MetricCollector<long> receivedCount,MetricCollector<long> receivedBytes,
            MetricCollector<double> encodingDuration,MetricCollector<double> decodingDuration) ProduceCollectors(Meter owningMeter,Type? messageType=null,string? channel = null)
        {
            var template = "messages";
            if (channel!=null)
                template = $"channels.{channel}";
            else if (messageType!=null)
                template = $"types.{messageType.GetCustomAttributes<MessageNameAttribute>().Select(mn => mn.Value).FirstOrDefault(messageType.Name)}.{messageType.GetCustomAttributes<MessageVersionAttribute>().Select(mc => mc.Version.ToString()).FirstOrDefault("0.0.0.0").Replace('.', '_')}";
            return (
                new MetricCollector<long>(owningMeter,$"{MeterName}.{template}.sent.count"),
                new MetricCollector<long>(owningMeter, $"{MeterName}.{template}.sent.bytes"),
                new MetricCollector<long>(owningMeter,$"{MeterName}.{template}.received.count"),
                new MetricCollector<long>(owningMeter, $"{MeterName}.{template}.received.bytes"),
                new MetricCollector<double>(owningMeter, $"{MeterName}.{template}.encodingduration"),
                new MetricCollector<double>(owningMeter, $"{MeterName}.{template}.decodingduration")
            );
        }
        
        private static void CheckMeasurement(IReadOnlyList<CollectedMeasurement<long>> readOnlyList, int count, long value)
        {
            Assert.AreEqual(count, readOnlyList.Count);
            if (count>0)
                Assert.AreEqual(value, readOnlyList[0].Value);
        }

        private static void CheckMeasurement(IReadOnlyList<CollectedMeasurement<double>> readOnlyList, int count, double value)
        {
            Assert.AreEqual(count, readOnlyList.Count);
            if (count>0)
                Assert.AreEqual(value, readOnlyList[0].Value);
        }

        private static void CheckMeasurementGreaterThan(IReadOnlyList<CollectedMeasurement<long>> readOnlyList, int count, long value)
        {
            Assert.AreEqual(count, readOnlyList.Count);
            Assert.IsTrue(value<readOnlyList[0].Value);
        }

        private static void CheckMeasurementGreaterThan(IReadOnlyList<CollectedMeasurement<double>> readOnlyList, int count, double value)
        {
            Assert.AreEqual(count, readOnlyList.Count);
            Assert.IsTrue(value<readOnlyList[0].Value);
        }

        private static void AreMeasurementsEquals(IReadOnlyList<CollectedMeasurement<long>> left, IReadOnlyList<CollectedMeasurement<long>> right)
            =>Assert.IsTrue(left.Select(v=>v.Value).SequenceEqual(right.Select(v=>v.Value)));

        private static void AreMeasurementsEquals(IReadOnlyList<CollectedMeasurement<double>> left, IReadOnlyList<CollectedMeasurement<double>> right)
            => Assert.IsTrue(left.Select(v => v.Value).SequenceEqual(right.Select(v => v.Value)));

        [TestMethod]
        public void TestSystemMetricInitialization()
        {
            #region Arrange
            var testMeter = new Meter("TestSystemMetricInitialization");
            var serviceConnection = new Mock<IMessageServiceConnection>();
            _ = ContractConnection.Instance(serviceConnection.Object)
                .AddMetrics(testMeter, false);
            #endregion

            #region Act
            (MetricCollector<long> sent, MetricCollector<long> sentBytes, MetricCollector<long> receivedCount, MetricCollector<long> recievedBytes,
            MetricCollector<double> encodingDuration, MetricCollector<double> decodingDuration) = ProduceCollectors(testMeter);
            #endregion

            #region Assert
            CheckMeasurement(sent.GetMeasurementSnapshot(), 0, 0);
            CheckMeasurement(sentBytes.GetMeasurementSnapshot(), 0, 0);
            CheckMeasurement(receivedCount.GetMeasurementSnapshot(), 0, 0);
            CheckMeasurement(recievedBytes.GetMeasurementSnapshot(), 0, 0);
            CheckMeasurement(encodingDuration.GetMeasurementSnapshot(), 0, 0);
            CheckMeasurement(decodingDuration.GetMeasurementSnapshot(), 0, 0);
            #endregion

            #region Verify
            #endregion
        }

        [TestMethod]
        public async Task TestPublishAsyncSubscribeSystemMetrics()
        {
            #region Arrange
            var testMeter = new Meter("TestPublishAsyncSubscribeSystemMetrics");
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();

            var testMessage = new BasicMessage("testMessage");
            var channel = "TestSystemMetricsChannel";

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
                .AddMetrics(testMeter,false);
            #endregion

            #region Act
            (MetricCollector<long> sent, MetricCollector<long> sentBytes, MetricCollector<long> receivedCount, MetricCollector<long> receivedBytes,
            MetricCollector<double> encodingDuration, MetricCollector<double> decodingDuration) = ProduceCollectors(testMeter);
            (MetricCollector<long> sentType, MetricCollector<long> sentBytesType, MetricCollector<long> receivedCountType, MetricCollector<long> receivedBytesType,
            MetricCollector<double> encodingDurationType, MetricCollector<double> decodingDurationType) = ProduceCollectors(testMeter,messageType: typeof(BasicMessage));
            (MetricCollector<long> sentChannel, MetricCollector<long> sentBytesChannel, MetricCollector<long> receivedCountChannel, MetricCollector<long> receivedBytesChannel,
            MetricCollector<double> encodingDurationChannel, MetricCollector<double> decodingDurationChannel) = ProduceCollectors(testMeter,channel: channel);
            _ = await contractConnection.SubscribeAsync<BasicMessage>(
                (msg) => ValueTask.CompletedTask, 
                (error) => { },
                channel:channel);
            var result = await contractConnection.PublishAsync<BasicMessage>(testMessage,channel:channel);
            _ = await Helper.WaitForCount<ServiceMessage>(messages, 1, TimeSpan.FromMinutes(1));
            #endregion

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            CheckMeasurement(sent.GetMeasurementSnapshot(), 1, 1);
            CheckMeasurementGreaterThan(sentBytes.GetMeasurementSnapshot(), 1, 0);
            CheckMeasurement(receivedCount.GetMeasurementSnapshot(), 1, 1);
            CheckMeasurementGreaterThan(receivedBytes.GetMeasurementSnapshot(), 1, 0);
            CheckMeasurementGreaterThan(encodingDuration.GetMeasurementSnapshot(), 1, 0);
            CheckMeasurementGreaterThan(decodingDuration.GetMeasurementSnapshot(), 1, 0);

            AreMeasurementsEquals(sent.GetMeasurementSnapshot(), sentType.GetMeasurementSnapshot());
            AreMeasurementsEquals(sentBytes.GetMeasurementSnapshot(), sentBytesType.GetMeasurementSnapshot());
            AreMeasurementsEquals(receivedCount.GetMeasurementSnapshot(), receivedCountType.GetMeasurementSnapshot());
            AreMeasurementsEquals(receivedBytes.GetMeasurementSnapshot(), receivedBytesType.GetMeasurementSnapshot());
            AreMeasurementsEquals(encodingDuration.GetMeasurementSnapshot(),encodingDurationType.GetMeasurementSnapshot());
            AreMeasurementsEquals(decodingDuration.GetMeasurementSnapshot(),decodingDurationType.GetMeasurementSnapshot());

            AreMeasurementsEquals(sent.GetMeasurementSnapshot(), sentChannel.GetMeasurementSnapshot());
            AreMeasurementsEquals(sentBytes.GetMeasurementSnapshot(), sentBytesChannel.GetMeasurementSnapshot());
            AreMeasurementsEquals(receivedCount.GetMeasurementSnapshot(), receivedCountChannel.GetMeasurementSnapshot());
            AreMeasurementsEquals(receivedBytes.GetMeasurementSnapshot(), receivedBytesChannel.GetMeasurementSnapshot());
            AreMeasurementsEquals(encodingDuration.GetMeasurementSnapshot(), encodingDurationChannel.GetMeasurementSnapshot());
            AreMeasurementsEquals(decodingDuration.GetMeasurementSnapshot(), decodingDurationChannel.GetMeasurementSnapshot());
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestSubscribeQueryResponseAsyncWithNoExtendedAspects()
        {
            #region Arrange
            var testMeter = new Meter("TestSubscribeQueryResponseAsyncWithNoExtendedAspects");
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
                .AddMetrics(testMeter, false);

            var message = new BasicQueryMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var responseMessage = new BasicResponseMessage("TestSubscribeQueryResponseWithNoExtendedAspects");
            var channel = "TestQueryMetricChannel";
            #endregion

            #region Act
            (MetricCollector<long> sent, MetricCollector<long> sentBytes, MetricCollector<long> receivedCount, MetricCollector<long> receivedBytes,
            MetricCollector<double> encodingDuration, MetricCollector<double> decodingDuration) = ProduceCollectors(testMeter);
            (MetricCollector<long> sentRequestType, MetricCollector<long> sentBytesRequestType, MetricCollector<long> receivedCountRequestType, MetricCollector<long> receivedBytesRequestType,
            MetricCollector<double> encodingDurationRequestType, MetricCollector<double> decodingDurationRequestType) = ProduceCollectors(testMeter,messageType: typeof(BasicQueryMessage));
            (MetricCollector<long> sentResponseType, MetricCollector<long> sentBytesResponseType, MetricCollector<long> receivedCountResponseType, MetricCollector<long> receivedBytesResponseType,
            MetricCollector<double> encodingDurationResponseType, MetricCollector<double> decodingDurationResponseType) = ProduceCollectors(testMeter,messageType: typeof(BasicResponseMessage));
            (MetricCollector<long> sentChannel, MetricCollector<long> sentBytesChannel, MetricCollector<long> receivedCountChannel, MetricCollector<long> receivedBytesChannel,
            MetricCollector<double> encodingDurationChannel, MetricCollector<double> decodingDurationChannel) = ProduceCollectors(testMeter,channel: channel);
            _ = await contractConnection.SubscribeQueryAsyncResponseAsync<BasicQueryMessage, BasicResponseMessage>((msg) =>
            {
                return ValueTask.FromResult(new QueryResponseMessage<BasicResponseMessage>(responseMessage, null));
            }, (error) => { },
            channel:channel);
            _ = await contractConnection.QueryAsync<BasicQueryMessage>(message,channel:channel);
            #endregion

            #region Assert
            CheckMeasurement(sent.GetMeasurementSnapshot(), 2, 1);
            CheckMeasurementGreaterThan(sentBytes.GetMeasurementSnapshot(), 2, 0);
            CheckMeasurement(receivedCount.GetMeasurementSnapshot(), 2, 1);
            CheckMeasurementGreaterThan(receivedBytes.GetMeasurementSnapshot(), 2, 0);
            CheckMeasurementGreaterThan(encodingDuration.GetMeasurementSnapshot(), 2, 0);
            CheckMeasurementGreaterThan(decodingDuration.GetMeasurementSnapshot(), 2, 0);

            CheckMeasurement(sentRequestType.GetMeasurementSnapshot(), 1, 1);
            CheckMeasurementGreaterThan(sentBytesRequestType.GetMeasurementSnapshot(), 1, 0);
            CheckMeasurement(receivedCountRequestType.GetMeasurementSnapshot(), 1, 1);
            CheckMeasurementGreaterThan(receivedBytesRequestType.GetMeasurementSnapshot(), 1, 0);
            CheckMeasurementGreaterThan(encodingDurationRequestType.GetMeasurementSnapshot(), 1, 0);
            CheckMeasurementGreaterThan(decodingDurationRequestType.GetMeasurementSnapshot(), 1, 0);

            CheckMeasurement(sentResponseType.GetMeasurementSnapshot(), 1, 1);
            CheckMeasurementGreaterThan(sentBytesResponseType.GetMeasurementSnapshot(), 1, 0);
            CheckMeasurement(receivedCountResponseType.GetMeasurementSnapshot(), 1, 1);
            CheckMeasurementGreaterThan(receivedBytesResponseType.GetMeasurementSnapshot(), 1, 0);
            CheckMeasurementGreaterThan(encodingDurationResponseType.GetMeasurementSnapshot(), 1, 0);
            CheckMeasurementGreaterThan(decodingDurationResponseType.GetMeasurementSnapshot(), 1, 0);

            AreMeasurementsEquals(sentRequestType.GetMeasurementSnapshot(), sentChannel.GetMeasurementSnapshot());
            AreMeasurementsEquals(sentBytesRequestType.GetMeasurementSnapshot(), sentBytesChannel.GetMeasurementSnapshot());
            AreMeasurementsEquals(receivedCountRequestType.GetMeasurementSnapshot(), receivedCountChannel.GetMeasurementSnapshot());
            AreMeasurementsEquals(receivedBytesRequestType.GetMeasurementSnapshot(), receivedBytesChannel.GetMeasurementSnapshot());
            AreMeasurementsEquals(encodingDurationRequestType.GetMeasurementSnapshot(), encodingDurationChannel.GetMeasurementSnapshot());
            AreMeasurementsEquals(decodingDurationRequestType.GetMeasurementSnapshot(), decodingDurationChannel.GetMeasurementSnapshot());
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.SubscribeQueryAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask<ServiceMessage>>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }
    }
}
