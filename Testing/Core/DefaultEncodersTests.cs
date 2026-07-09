using Moq;
using MQContract;
using MQContract.Interfaces;
using MQContract.Interfaces.Service;
using System.Security.Cryptography;

namespace CoreTesting;

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

        List<ReceivedServiceMessage> serviceMessages = [];
        var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
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
        var subscription = await contractConnection.SubscribeAsync<byte[]>((msg) => recievedMessages.Add(msg), (err) => { }, ChannelName, cancellationToken: TestContext.CancellationToken);
        var result = await contractConnection.PublishAsync<byte[]>(new TransmissionMessage<byte[]>(testMessage), ChannelName, cancellationToken: TestContext.CancellationToken);
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
        serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        #endregion
    }

    [TestMethod]
    public async Task TestStringEncoder()
    {
        #region Arrange
        var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
        var serviceSubscription = new Mock<IServiceSubscription>();

        var testMessage = "The quick brown fox jumps over the lazy dog";

        List<ReceivedServiceMessage> serviceMessages = [];
        var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
        var recievedMessages = new List<IReceivedMessage<string>>();

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
        var subscription = await contractConnection.SubscribeAsync<string>((msg) => recievedMessages.Add(msg), (err) => { }, ChannelName, cancellationToken: TestContext.CancellationToken);
        var result = await contractConnection.PublishAsync<string>(new TransmissionMessage<string>(testMessage), ChannelName, cancellationToken: TestContext.CancellationToken);
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(recievedMessages, 1, TimeSpan.FromMinutes(1)));

        await subscription.EndAsync();

        Assert.IsNotNull(result);
        Assert.AreEqual(transmissionResult, result);
        Assert.AreEqual(testMessage, recievedMessages[0].Message);
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms);
        await writer.WriteAsync(testMessage);
        await writer.FlushAsync(TestContext.CancellationToken);
        Assert.IsTrue(Enumerable.SequenceEqual<byte>(ms.ToArray(), serviceMessages[0].Data.ToArray()));
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        #endregion
    }

    private async Task BitConverterTypeTest<T>(T testMessage, byte[] convertedValue)
    {
        #region Arrange
        var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
        var serviceSubscription = new Mock<IServiceSubscription>();

        List<ReceivedServiceMessage> serviceMessages = [];
        var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
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
        var result = await contractConnection.PublishAsync<T>(new TransmissionMessage<T>(testMessage), ChannelName);
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
        serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        #endregion
    }

    private async Task BitConverterTypeTest<T>(IEnumerable<T> testMessages, byte[] convertedValue)
    {
        #region Arrange
        var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());
        var serviceSubscription = new Mock<IServiceSubscription>();

        List<ReceivedServiceMessage> serviceMessages = [];
        var actions = new List<Func<ReceivedServiceMessage, ValueTask>>();
        var recievedMessages = new List<IReceivedMessage<T>>();
        var recievedArrayMessages = new List<IReceivedMessage<T[]>>();
        var recievedEnumerableMessages = new List<IReceivedMessage<IEnumerable<T>>>();
        var channels = new List<string>();

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.SubscribeAsync(Capture.In(actions), It.IsAny<Action<Exception>>(), Capture.In(channels),
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceSubscription.Object);
        serviceConnection.Setup(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()))
            .Returns((ServiceMessage message, CancellationToken cancellationToken) =>
            {
                var rmessage = Helper.ProduceReceivedServiceMessage(message);
                serviceMessages.Add(rmessage);
                actions[channels.IndexOf(message.Channel)](rmessage);
                return ValueTask.FromResult(transmissionResult);
            });

        var contractConnection = ContractConnection.Instance(serviceConnection.Object);
        #endregion

        #region Act
        var subscription = await contractConnection.SubscribeAsync<T>((msg) => recievedMessages.Add(msg), (err) => { }, ChannelName);
        var subscriptionArray = await contractConnection.SubscribeAsync<T[]>((msg) => recievedArrayMessages.Add(msg), (err) => { }, $"{ChannelName}_array");
        var subscriptionEnumerable = await contractConnection.SubscribeAsync<IEnumerable<T>>((msg) => recievedEnumerableMessages.Add(msg), (err) => { }, $"{ChannelName}_enumerable");
        var result = await contractConnection.PublishAsync<T>(new TransmissionMessage<T>(testMessages.First()), ChannelName);
        var resultArray = await contractConnection.PublishAsync<T[]>(new TransmissionMessage<T[]>(testMessages.ToArray()), $"{ChannelName}_array");
        var resultEnumerable = await contractConnection.PublishAsync<IEnumerable<T>>(new TransmissionMessage<IEnumerable<T>>(testMessages), $"{ChannelName}_enumerable");
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(recievedMessages, 1, TimeSpan.FromMinutes(1)));
        Assert.IsTrue(await Helper.WaitForCount(recievedArrayMessages, 1, TimeSpan.FromMinutes(1)));
        Assert.IsTrue(await Helper.WaitForCount(recievedEnumerableMessages, 1, TimeSpan.FromMinutes(1)));

        await subscription.EndAsync();
        await subscriptionArray.EndAsync();
        await subscriptionEnumerable.EndAsync();

        Assert.IsNotNull(result);
        Assert.IsNotNull(resultArray);
        Assert.IsNotNull(resultEnumerable);
        Assert.AreEqual(transmissionResult, result);
        Assert.AreEqual(transmissionResult, resultArray);
        Assert.AreEqual(transmissionResult, resultEnumerable);
        Assert.AreEqual(testMessages.First(), recievedMessages[0].Message);
        Assert.IsTrue(Enumerable.SequenceEqual<T>(testMessages, recievedArrayMessages[0].Message));
        Assert.IsTrue(Enumerable.SequenceEqual<T>(testMessages, recievedEnumerableMessages[0].Message));
        Assert.IsTrue(Enumerable.SequenceEqual<byte>(convertedValue.Take(convertedValue.Length/3), serviceMessages[0].Data.ToArray()));
        Assert.IsTrue(Enumerable.SequenceEqual<byte>(convertedValue, serviceMessages[1].Data.ToArray()));
        Assert.IsTrue(Enumerable.SequenceEqual<byte>(convertedValue, serviceMessages[2].Data.ToArray()));
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        serviceConnection.Verify(x => x.SubscribeAsync(It.IsAny<Func<ReceivedServiceMessage, ValueTask>>(), It.IsAny<Action<Exception>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
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
    public async Task TestBooleanEncoder()
    {
        var binaryData = BitConverter.GetBytes(true).Concat(BitConverter.GetBytes(false)).Concat(BitConverter.GetBytes(false));
        await BitConverterTypeTest<bool>([true, false, false], binaryData.ToArray());
    }

    [TestMethod]
    public async Task TestDoubleEncoder()
    {
        var binaryData = RandomNumberGenerator.GetBytes(sizeof(double)*3);
        await BitConverterTypeTest<double>([BitConverter.ToDouble(binaryData, 0), BitConverter.ToDouble(binaryData, sizeof(double)), BitConverter.ToDouble(binaryData, sizeof(double)*2)], binaryData);
    }

    [TestMethod]
    public async Task TestFloatEncoder()
    {
        var binaryData = RandomNumberGenerator.GetBytes(sizeof(float)*3);
        await BitConverterTypeTest<float>([BitConverter.ToSingle(binaryData), BitConverter.ToSingle(binaryData, sizeof(float)), BitConverter.ToSingle(binaryData, sizeof(float)*2)], binaryData);
    }

    [TestMethod]
    public async Task TestHalfEncoder()
    {
        var binaryData = RandomNumberGenerator.GetBytes(2*3);
        await BitConverterTypeTest<Half>([BitConverter.ToHalf(binaryData), BitConverter.ToHalf(binaryData, 2), BitConverter.ToHalf(binaryData, 4)], binaryData);
    }

    [TestMethod]
    public async Task TestIntEncoder()
    {
        var binaryData = RandomNumberGenerator.GetBytes(sizeof(int)*3);
        await BitConverterTypeTest<int>([BitConverter.ToInt32(binaryData), BitConverter.ToInt32(binaryData, sizeof(int)), BitConverter.ToInt32(binaryData, sizeof(int)*2)], binaryData);
    }

    [TestMethod]
    public async Task TestLongEncoder()
    {
        var binaryData = RandomNumberGenerator.GetBytes(sizeof(long)*3);
        await BitConverterTypeTest<long>([BitConverter.ToInt64(binaryData), BitConverter.ToInt64(binaryData, sizeof(long)), BitConverter.ToInt64(binaryData, sizeof(long)*2)], binaryData);
    }

    [TestMethod]
    public async Task TestShortEncoder()
    {
        var binaryData = RandomNumberGenerator.GetBytes(sizeof(short)*3);
        await BitConverterTypeTest<short>([BitConverter.ToInt16(binaryData), BitConverter.ToInt16(binaryData, sizeof(short)), BitConverter.ToInt16(binaryData, sizeof(short)*2)], binaryData);
    }

    [TestMethod]
    public async Task TestUIntEncoder()
    {
        var binaryData = RandomNumberGenerator.GetBytes(sizeof(uint) * 3);
        await BitConverterTypeTest<uint>([BitConverter.ToUInt32(binaryData), BitConverter.ToUInt32(binaryData, sizeof(uint)), BitConverter.ToUInt32(binaryData, sizeof(uint)*2)], binaryData);
    }

    [TestMethod]
    public async Task TestULongEncoder()
    {
        var binaryData = RandomNumberGenerator.GetBytes(sizeof(ulong) * 3);
        await BitConverterTypeTest<ulong>([BitConverter.ToUInt64(binaryData), BitConverter.ToUInt64(binaryData, sizeof(ulong)), BitConverter.ToUInt64(binaryData, sizeof(ulong)*2)], binaryData);
    }

    [TestMethod]
    public async Task TestUShortEncoder()
    {
        var binaryData = RandomNumberGenerator.GetBytes(sizeof(ushort)*3);
        await BitConverterTypeTest<ushort>([BitConverter.ToUInt16(binaryData), BitConverter.ToUInt16(binaryData, sizeof(ushort)), BitConverter.ToUInt16(binaryData, sizeof(ushort)*2)], binaryData);
    }

    [TestMethod]
    public async Task TestDecimalEncoder()
    {
        var values = new decimal[]{
            new(
                BitConverter.ToInt32(RandomNumberGenerator.GetBytes(sizeof(int))),
                BitConverter.ToInt32(RandomNumberGenerator.GetBytes(sizeof(int))),
                BitConverter.ToInt32(RandomNumberGenerator.GetBytes(sizeof(int))),
                RandomNumberGenerator.GetBytes(1)[0]<(byte.MaxValue/2),
                (byte)(RandomNumberGenerator.GetBytes(1)[0]%28)
            ),
            new(
                BitConverter.ToInt32(RandomNumberGenerator.GetBytes(sizeof(int))),
                BitConverter.ToInt32(RandomNumberGenerator.GetBytes(sizeof(int))),
                BitConverter.ToInt32(RandomNumberGenerator.GetBytes(sizeof(int))),
                RandomNumberGenerator.GetBytes(1)[0]<(byte.MaxValue/2),
                (byte)(RandomNumberGenerator.GetBytes(1)[0]%28)
            ),
            new(
                BitConverter.ToInt32(RandomNumberGenerator.GetBytes(sizeof(int))),
                BitConverter.ToInt32(RandomNumberGenerator.GetBytes(sizeof(int))),
                BitConverter.ToInt32(RandomNumberGenerator.GetBytes(sizeof(int))),
                RandomNumberGenerator.GetBytes(1)[0]<(byte.MaxValue/2),
                (byte)(RandomNumberGenerator.GetBytes(1)[0]%28)
            )
        };
        using var ms = new MemoryStream();
        foreach (var d in values)
        {
            foreach (var b in decimal.GetBits(d))
                await ms.WriteAsync(BitConverter.GetBytes(b), cancellationToken: TestContext.CancellationToken);
        }

        await BitConverterTypeTest<decimal>(values, ms.ToArray());
    }

    public TestContext TestContext { get; set; }
}
