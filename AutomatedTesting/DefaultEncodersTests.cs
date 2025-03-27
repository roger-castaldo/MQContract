using Moq;
using MQContract;
using MQContract.Interfaces;
using MQContract.Interfaces.Service;
using System.Security.Cryptography;

namespace AutomatedTesting
{
    [TestClass]
    public class DefaultEncodersTests
    {
        private const string ChannelName = "TestDirectEncoders";

        [TestMethod]
        public async Task TestByteArrayEncoder()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();

            var testMessage = RandomNumberGenerator.GetBytes(1024);

            List<ServiceMessage> serviceMessages = [];
            var actions = new List<Action<ReceivedServiceMessage>>();
            var recievedMessages = new List<IReceivedMessage<byte[]>>();

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeAsync<byte[]>((msg) => recievedMessages.Add(msg), (err) => { }, ChannelName);
            var result = await contractConnection.PublishAsync<byte[]>(testMessage, ChannelName);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(recievedMessages, 1, TimeSpan.FromMinutes(1)));

            await subscription.EndAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.IsTrue(Enumerable.SequenceEqual<byte>(testMessage, recievedMessages[0].Message));
            Assert.IsTrue(Enumerable.SequenceEqual<byte>(testMessage, serviceMessages[0].Data.ToArray()));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestBooleanEncoder()
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();

            var testMessage = true;

            List<ServiceMessage> serviceMessages = [];
            var actions = new List<Action<ReceivedServiceMessage>>();
            var recievedMessages = new List<IReceivedMessage<bool>>();

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeAsync<bool>((msg) => recievedMessages.Add(msg), (err) => { }, ChannelName);
            var result = await contractConnection.PublishAsync<bool>(testMessage, ChannelName);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(recievedMessages, 1, TimeSpan.FromMinutes(1)));

            await subscription.EndAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(testMessage, recievedMessages[0].Message);
            Assert.IsTrue(Enumerable.SequenceEqual<byte>(BitConverter.GetBytes(testMessage), serviceMessages[0].Data.ToArray()));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        private async Task BitConverterTypeTest<T>(T testMessage, byte[] convertedValue)
        {
            #region Arrange
            var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
            var serviceSubscription = new Mock<IServiceSubscription>();

            List<ServiceMessage> serviceMessages = [];
            var actions = new List<Action<ReceivedServiceMessage>>();
            var recievedMessages = new List<IReceivedMessage<T>>();

            var serviceConnection = new Mock<IMessageServiceConnection>();
            serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), It.IsAny<Action<Exception>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceSubscription.Object);
            serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
                .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
                {
                    var rmessage = Helper.ProduceReceivedServiceMessage(message);
                    serviceMessages.Add(rmessage);
                    foreach (var act in actions)
                        act(rmessage);
                    return ValueTask.FromResult(transmissionResult);
                });

            var contractConnection = ContractConnection.Instance(serviceConnection.Object);
            #endregion

            #region Act
            var subscription = await contractConnection.SubscribeAsync<T>((msg) => recievedMessages.Add(msg), (err) => { }, ChannelName);
            var result = await contractConnection.PublishAsync<T>(testMessage, ChannelName);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(recievedMessages, 1, TimeSpan.FromMinutes(1)));

            await subscription.EndAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(transmissionResult, result);
            Assert.AreEqual(testMessage, recievedMessages[0].Message);
            Assert.IsTrue(Enumerable.SequenceEqual<byte>(convertedValue, serviceMessages[0].Data.ToArray()));
            #endregion

            #region Verify
            serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Action<ReceivedServiceMessage>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestByteEncoder()
        {
            var binaryData = RandomNumberGenerator.GetBytes(sizeof(byte));
            await BitConverterTypeTest<byte>(binaryData[0], binaryData);
        }

        [TestMethod]
        public async Task TestCharEncoder()
        {
            var binaryData = RandomNumberGenerator.GetBytes(sizeof(char));
            await BitConverterTypeTest<char>(BitConverter.ToChar(binaryData), binaryData);
        }

        [TestMethod]
        public async Task TestDoubleEncoder()
        {
            var binaryData = RandomNumberGenerator.GetBytes(sizeof(double));
            await BitConverterTypeTest<double>(BitConverter.ToDouble(binaryData), binaryData);
        }

        [TestMethod]
        public async Task TestFloatEncoder()
        {
            var binaryData = RandomNumberGenerator.GetBytes(sizeof(float));
            await BitConverterTypeTest<float>(BitConverter.ToSingle(binaryData), binaryData);
        }

        [TestMethod]
        public async Task TestHalfEncoder()
        {
            var binaryData = RandomNumberGenerator.GetBytes(2);
            await BitConverterTypeTest<Half>(BitConverter.ToHalf(binaryData), binaryData);
        }

        [TestMethod]
        public async Task TestIntEncoder()
        {
            var binaryData = RandomNumberGenerator.GetBytes(sizeof(int));
            await BitConverterTypeTest<int>(BitConverter.ToInt32(binaryData), binaryData);
        }

        [TestMethod]
        public async Task TestLongEncoder()
        {
            var binaryData = RandomNumberGenerator.GetBytes(sizeof(long));
            await BitConverterTypeTest<long>(BitConverter.ToInt64(binaryData), binaryData);
        }

        [TestMethod]
        public async Task TestShortEncoder()
        {
            var binaryData = RandomNumberGenerator.GetBytes(sizeof(short));
            await BitConverterTypeTest<short>(BitConverter.ToInt16(binaryData), binaryData);
        }

        [TestMethod]
        public async Task TestUIntEncoder()
        {
            var binaryData = RandomNumberGenerator.GetBytes(sizeof(uint));
            await BitConverterTypeTest<uint>(BitConverter.ToUInt32(binaryData), binaryData);
        }

        [TestMethod]
        public async Task TestULongEncoder()
        {
            var binaryData = RandomNumberGenerator.GetBytes(sizeof(ulong));
            await BitConverterTypeTest<ulong>(BitConverter.ToUInt64(binaryData), binaryData);
        }

        [TestMethod]
        public async Task TestUShortEncoder()
        {
            var binaryData = RandomNumberGenerator.GetBytes(sizeof(ushort));
            await BitConverterTypeTest<ushort>(BitConverter.ToUInt16(binaryData), binaryData);
        }

        [TestMethod]
        public async Task TestDecimalEncoder()
        {
            var value = new decimal(
                BitConverter.ToInt32(RandomNumberGenerator.GetBytes(sizeof(int))),
                BitConverter.ToInt32(RandomNumberGenerator.GetBytes(sizeof(int))),
                BitConverter.ToInt32(RandomNumberGenerator.GetBytes(sizeof(int))),
                RandomNumberGenerator.GetBytes(1)[0]<(byte.MaxValue/2),
                (byte)(RandomNumberGenerator.GetBytes(1)[0]%28)
            );
            var bits = decimal.GetBits(value);
            var binaryData = new byte[sizeof(int)*bits.Length];
            for (var i = 0; i<bits.Length; i++)
                Buffer.BlockCopy(BitConverter.GetBytes(bits[i]), 0, binaryData, i*sizeof(int), sizeof(int));

            await BitConverterTypeTest<decimal>(value, binaryData);
        }
    }
}
