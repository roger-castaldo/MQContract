using CoreTesting.Messages;
using Moq;
using MQContract;
using MQContract.Interfaces.Service;
using System.Text.Json;

namespace CoreTesting.ConnectionTests;

[TestClass]
public class MappedCallsTests
{
    private const string ServiceName = "testService";
    [TestMethod]
    public async Task TestPublishSingleServiceAsync()
    {
        #region Arrange
        var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

        var testMessage = new BasicMessage("testMessage");
        var channel = "testChannel";
        var headers = new MessageHeader([new KeyValuePair<string, string?>("key1", "value1")]);
        var cancellationTokenSource = new CancellationTokenSource();

        List<ServiceMessage> messages = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transmissionResult);

        var contractConnection = ContractConnection.Instance(serviceConnection.Object);
        #endregion

        #region Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result1 = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel, messageHeader: headers, cancellationToken: cancellationTokenSource.Token);
#pragma warning restore CS0618 // Type or member is obsolete
        var result2 = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel, cancellationToken: cancellationTokenSource.Token);
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreEqual(transmissionResult, result1);
        Assert.AreEqual(transmissionResult, result2);
        Assert.IsTrue(messages.All(m=>Equals(m.Channel, channel)));
        Assert.IsTrue(messages[0].Header.AsEnumerable().SequenceEqual(headers.AsEnumerable()));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[1].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PublishAsync(It.Is<ServiceMessage>(sm => sm.Channel == channel && sm.Header.AsEnumerable().SequenceEqual(headers.AsEnumerable())), cancellationTokenSource.Token), Times.Once);
        serviceConnection.Verify(x => x.PublishAsync(It.Is<ServiceMessage>(sm => sm.Channel == channel && sm.Header.Count==0), cancellationTokenSource.Token), Times.Once);
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        #endregion
    }

    [TestMethod]
    public async Task TestBulkPublishSingleServiceAsync()
    {
        #region Arrange
        var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

        var testMessage = new BasicMessage("testMessage");
        var channel = "testChannel";
        var headers = new MessageHeader([new KeyValuePair<string, string?>("key1", "value1")]);
        var cancellationTokenSource = new CancellationTokenSource();

        List<IEnumerable<ServiceMessage>> messages = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.BulkPublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
            .ReturnsAsync([transmissionResult]);

        var contractConnection = ContractConnection.Instance(serviceConnection.Object);
        #endregion

        #region Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result1 = await contractConnection.BulkPublishAsync<BasicMessage>([(message:testMessage, messageHeader:headers)], channel: channel, cancellationToken: cancellationTokenSource.Token);
#pragma warning restore CS0618 // Type or member is obsolete
        var result2 = await contractConnection.BulkPublishAsync<BasicMessage>([testMessage], channel: channel, cancellationToken: cancellationTokenSource.Token);
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreEqual(transmissionResult, result1.ElementAt(0));
        Assert.AreEqual(transmissionResult, result2.ElementAt(0));
        Assert.IsTrue(messages.All(msgs => msgs.All(m=>Equals(m.Channel, channel))));
        Assert.IsTrue(messages[0].ElementAt(0).Header.AsEnumerable().SequenceEqual(headers.AsEnumerable()));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].ElementAt(0).Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[1].ElementAt(0).Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.BulkPublishAsync(It.Is<IEnumerable<ServiceMessage>>(msgs => msgs.All(m => m.Channel == channel && m.Header.AsEnumerable().SequenceEqual(headers.AsEnumerable()))), cancellationTokenSource.Token), Times.Once);
        serviceConnection.Verify(x => x.BulkPublishAsync(It.Is<IEnumerable<ServiceMessage>>(msgs => msgs.All(m => m.Channel == channel && m.Header.Count == 0)), cancellationTokenSource.Token), Times.Once);
        serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        #endregion
    }

    [TestMethod]
    public async Task TestQuerySingleServiceAsync()
    {
        #region Arrange
        var testMessage = new BasicQueryMessage("testMessage");
        var responseMessage = new BasicResponseMessage("testResponse");

        var channel = "testChannel";
        var responseChannel = "responseChannel";
        var timeout = TimeSpan.FromSeconds(30);
        var headers = new MessageHeader([new KeyValuePair<string, string?>("key1", "value1")]);
        var cancellationTokenSource = new CancellationTokenSource();

        using var ms = new MemoryStream();
        await JsonSerializer.SerializeAsync(ms, responseMessage, cancellationToken: cancellationTokenSource.Token);
        var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

        var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);

        List<ServiceMessage> messages = [];
        List<TimeSpan> timeouts = [];

        var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
        serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        var contractConnection = ContractConnection.Instance(serviceConnection.Object);
        #endregion

        #region Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result1 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, timeout: timeout, channel: channel, responseChannel: responseChannel, messageHeader: headers, cancellationToken: cancellationTokenSource.Token);
        var result2 = await contractConnection.QueryAsync<BasicQueryMessage>(testMessage, timeout: timeout, channel: channel, responseChannel: responseChannel, messageHeader: headers, cancellationToken: cancellationTokenSource.Token);
#pragma warning restore CS0618 // Type or member is obsolete
        var result3 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, timeout: timeout, channel: channel, responseChannel: responseChannel, cancellationToken: cancellationTokenSource.Token);
        var result4 = await contractConnection.QueryAsync<BasicQueryMessage>(testMessage, timeout: timeout, channel: channel, responseChannel: responseChannel, cancellationToken: cancellationTokenSource.Token);
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, 4, TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(result1);
        Assert.AreEqual(queryResult.ID, result1.ID);
        Assert.IsNull(result1.Error);
        Assert.IsFalse(result1.IsError);
        Assert.IsNotNull(result2);
        Assert.AreEqual(queryResult.ID, result2.ID);
        Assert.IsNull(result2.Error);
        Assert.IsFalse(result2.IsError);
        Assert.IsNotNull(result3);
        Assert.AreEqual(queryResult.ID, result3.ID);
        Assert.IsNull(result3.Error);
        Assert.IsFalse(result3.IsError);
        Assert.IsNotNull(result4);
        Assert.AreEqual(queryResult.ID, result4.ID);
        Assert.IsNull(result4.Error);
        Assert.IsFalse(result4.IsError);
        Assert.IsTrue(messages.All(m => Equals(m.Channel, channel)));
        Assert.HasCount(4, timeouts);
        Assert.IsTrue(timeouts.All(t => t == timeout));
        Assert.IsTrue(messages[0].Header.AsEnumerable().SequenceEqual(headers.AsEnumerable()));
        Assert.IsTrue(messages[1].Header.AsEnumerable().SequenceEqual(headers.AsEnumerable()));
        Assert.AreEqual(0, messages[2].Header.Count);
        Assert.AreEqual(0, messages[3].Header.Count);
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[1].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[2].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[3].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.QueryAsync(It.Is<ServiceMessage>(sm => sm.Channel == channel && sm.Header.AsEnumerable().SequenceEqual(headers.AsEnumerable())), timeout, cancellationTokenSource.Token), Times.Exactly(2));
        serviceConnection.Verify(x => x.QueryAsync(It.Is<ServiceMessage>(sm => sm.Channel == channel && sm.Header.Count==0), timeout, cancellationTokenSource.Token), Times.Exactly(2));
        serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
        #endregion
    }

    [TestMethod]
    public async Task TestPublishMappedServiceAsync()
    {
        #region Arrange
        var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

        var testMessage = new BasicMessage("testMessage");
        var channel = "testChannel";
        var headers = new MessageHeader([new KeyValuePair<string, string?>("key1", "value1")]);
        var cancellationTokenSource = new CancellationTokenSource();

        List<ServiceMessage> messages = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transmissionResult);

        var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props)=>true, ServiceName, serviceConnection.Object);
        #endregion

        #region Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result1 = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel, messageHeader: headers, cancellationToken: cancellationTokenSource.Token);
#pragma warning restore CS0618 // Type or member is obsolete
        var result2 = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel, cancellationToken: cancellationTokenSource.Token);
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreEqual(transmissionResult, result1);
        Assert.AreEqual(transmissionResult, result2);
        Assert.IsTrue(messages.All(m => Equals(m.Channel, channel)));
        Assert.IsTrue(messages[0].Header.AsEnumerable().SequenceEqual(headers.AsEnumerable()));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[1].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PublishAsync(It.Is<ServiceMessage>(sm => sm.Channel == channel && sm.Header.AsEnumerable().SequenceEqual(headers.AsEnumerable())), cancellationTokenSource.Token), Times.Once);
        serviceConnection.Verify(x => x.PublishAsync(It.Is<ServiceMessage>(sm => sm.Channel == channel && sm.Header.Count==0), cancellationTokenSource.Token), Times.Once);
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        #endregion
    }

    [TestMethod]
    public async Task TestBulkPublishMappedServiceAsync()
    {
        #region Arrange
        var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

        var testMessage = new BasicMessage("testMessage");
        var channel = "testChannel";
        var headers = new MessageHeader([new KeyValuePair<string, string?>("key1", "value1")]);
        var cancellationTokenSource = new CancellationTokenSource();

        List<IEnumerable<ServiceMessage>> messages = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.BulkPublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
            .ReturnsAsync([transmissionResult]);

        var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
        #endregion

        #region Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result1 = await contractConnection.BulkPublishAsync<BasicMessage>([(message: testMessage, messageHeader: headers)], channel: channel, cancellationToken: cancellationTokenSource.Token);
#pragma warning restore CS0618 // Type or member is obsolete
        var result2 = await contractConnection.BulkPublishAsync<BasicMessage>([testMessage], channel: channel, cancellationToken: cancellationTokenSource.Token);
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreEqual(transmissionResult, result1.ElementAt(0));
        Assert.AreEqual(transmissionResult, result2.ElementAt(0));
        Assert.IsTrue(messages.All(msgs => msgs.All(m => Equals(m.Channel, channel))));
        Assert.IsTrue(messages[0].ElementAt(0).Header.AsEnumerable().SequenceEqual(headers.AsEnumerable()));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].ElementAt(0).Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[1].ElementAt(0).Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.BulkPublishAsync(It.Is<IEnumerable<ServiceMessage>>(msgs => msgs.All(m => m.Channel == channel && m.Header.AsEnumerable().SequenceEqual(headers.AsEnumerable()))), cancellationTokenSource.Token), Times.Once);
        serviceConnection.Verify(x => x.BulkPublishAsync(It.Is<IEnumerable<ServiceMessage>>(msgs => msgs.All(m => m.Channel == channel && m.Header.Count == 0)), cancellationTokenSource.Token), Times.Once);
        serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        #endregion
    }

    [TestMethod]
    public async Task TestQueryMappedServiceAsync()
    {
        #region Arrange
        var testMessage = new BasicQueryMessage("testMessage");
        var responseMessage = new BasicResponseMessage("testResponse");

        var channel = "testChannel";
        var responseChannel = "responseChannel";
        var timeout = TimeSpan.FromSeconds(30);
        var headers = new MessageHeader([new KeyValuePair<string, string?>("key1", "value1")]);
        var cancellationTokenSource = new CancellationTokenSource();

        using var ms = new MemoryStream();
        await JsonSerializer.SerializeAsync(ms, responseMessage, cancellationToken: cancellationTokenSource.Token);
        var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

        var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);

        List<ServiceMessage> messages = [];
        List<TimeSpan> timeouts = [];

        var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
        serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        var contractConnection = ContractConnection.MappedServiceInstance().RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
        #endregion

        #region Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result1 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, timeout: timeout, channel: channel, responseChannel: responseChannel, messageHeader: headers, cancellationToken: cancellationTokenSource.Token);
        var result2 = await contractConnection.QueryAsync<BasicQueryMessage>(testMessage, timeout: timeout, channel: channel, responseChannel: responseChannel, messageHeader: headers, cancellationToken: cancellationTokenSource.Token);
#pragma warning restore CS0618 // Type or member is obsolete
        var result3 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, timeout: timeout, channel: channel, responseChannel: responseChannel, cancellationToken: cancellationTokenSource.Token);
        var result4 = await contractConnection.QueryAsync<BasicQueryMessage>(testMessage, timeout: timeout, channel: channel, responseChannel: responseChannel, cancellationToken: cancellationTokenSource.Token);
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, 4, TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(result1);
        Assert.AreEqual(queryResult.ID, result1.ID);
        Assert.IsNull(result1.Error);
        Assert.IsFalse(result1.IsError);
        Assert.IsNotNull(result2);
        Assert.AreEqual(queryResult.ID, result2.ID);
        Assert.IsNull(result2.Error);
        Assert.IsFalse(result2.IsError);
        Assert.IsNotNull(result3);
        Assert.AreEqual(queryResult.ID, result3.ID);
        Assert.IsNull(result3.Error);
        Assert.IsFalse(result3.IsError);
        Assert.IsNotNull(result4);
        Assert.AreEqual(queryResult.ID, result4.ID);
        Assert.IsNull(result4.Error);
        Assert.IsFalse(result4.IsError);
        Assert.IsTrue(messages.All(m => Equals(m.Channel, channel)));
        Assert.HasCount(4, timeouts);
        Assert.IsTrue(timeouts.All(t => t == timeout));
        Assert.IsTrue(messages[0].Header.AsEnumerable().SequenceEqual(headers.AsEnumerable()));
        Assert.IsTrue(messages[1].Header.AsEnumerable().SequenceEqual(headers.AsEnumerable()));
        Assert.AreEqual(0, messages[2].Header.Count);
        Assert.AreEqual(0, messages[3].Header.Count);
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[1].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[2].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[3].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.QueryAsync(It.Is<ServiceMessage>(sm => sm.Channel == channel && sm.Header.AsEnumerable().SequenceEqual(headers.AsEnumerable())), timeout, cancellationTokenSource.Token), Times.Exactly(2));
        serviceConnection.Verify(x => x.QueryAsync(It.Is<ServiceMessage>(sm => sm.Channel == channel && sm.Header.Count==0), timeout, cancellationTokenSource.Token), Times.Exactly(2));
        serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
        #endregion
    }

    [TestMethod]
    public async Task TestPublishMultiServiceAsync()
    {
        #region Arrange
        var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

        var testMessage = new BasicMessage("testMessage");
        var channel = "testChannel";
        var headers = new MessageHeader([new KeyValuePair<string, string?>("key1", "value1")]);
        var cancellationTokenSource = new CancellationTokenSource();

        List<ServiceMessage> messages = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.PublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transmissionResult);

        var contractConnection = ContractConnection.MultiServiceInstance()
            .RegisterServiceConnection(ServiceName, serviceConnection.Object);
        #endregion

        #region Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result1 = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel, messageHeader: headers, cancellationToken: cancellationTokenSource.Token);
#pragma warning restore CS0618 // Type or member is obsolete
        var result2 = await contractConnection.PublishAsync<BasicMessage>(testMessage, channel: channel, cancellationToken: cancellationTokenSource.Token);
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.IsTrue(messages.All(m => Equals(m.Channel, channel)));
        Assert.IsTrue(messages[0].Header.AsEnumerable().SequenceEqual(headers.AsEnumerable()));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[1].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PublishAsync(It.Is<ServiceMessage>(sm => sm.Channel == channel && sm.Header.AsEnumerable().SequenceEqual(headers.AsEnumerable())), cancellationTokenSource.Token), Times.Once);
        serviceConnection.Verify(x => x.PublishAsync(It.Is<ServiceMessage>(sm => sm.Channel == channel && sm.Header.Count==0), cancellationTokenSource.Token), Times.Once);
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        #endregion
    }

    [TestMethod]
    public async Task TestBulkPublishMultiServiceAsync()
    {
        #region Arrange
        var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

        var testMessage = new BasicMessage("testMessage");
        var channel = "testChannel";
        var headers = new MessageHeader([new KeyValuePair<string, string?>("key1", "value1")]);
        var cancellationTokenSource = new CancellationTokenSource();

        List<IEnumerable<ServiceMessage>> messages = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.BulkPublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
            .ReturnsAsync([transmissionResult]);

        var contractConnection = ContractConnection.MultiServiceInstance()
            .RegisterServiceConnection(ServiceName, serviceConnection.Object);
        #endregion

        #region Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result1 = await contractConnection.BulkPublishAsync<BasicMessage>([(message: testMessage, messageHeader: headers)], channel: channel, cancellationToken: cancellationTokenSource.Token);
#pragma warning restore CS0618 // Type or member is obsolete
        var result2 = await contractConnection.BulkPublishAsync<BasicMessage>([testMessage], channel: channel, cancellationToken: cancellationTokenSource.Token);
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, 2, TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.IsTrue(messages.All(msgs => msgs.All(m => Equals(m.Channel, channel))));
        Assert.IsTrue(messages[0].ElementAt(0).Header.AsEnumerable().SequenceEqual(headers.AsEnumerable()));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].ElementAt(0).Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[1].ElementAt(0).Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.BulkPublishAsync(It.Is<IEnumerable<ServiceMessage>>(msgs => msgs.All(m => m.Channel == channel && m.Header.AsEnumerable().SequenceEqual(headers.AsEnumerable()))), cancellationTokenSource.Token), Times.Once);
        serviceConnection.Verify(x => x.BulkPublishAsync(It.Is<IEnumerable<ServiceMessage>>(msgs => msgs.All(m => m.Channel == channel && m.Header.Count == 0)), cancellationTokenSource.Token), Times.Once);
        serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        #endregion
    }

    [TestMethod]
    public async Task TestQueryMultiServiceAsync()
    {
        #region Arrange
        var testMessage = new BasicQueryMessage("testMessage");
        var responseMessage = new BasicResponseMessage("testResponse");

        var channel = "testChannel";
        var responseChannel = "responseChannel";
        var timeout = TimeSpan.FromSeconds(30);
        var headers = new MessageHeader([new KeyValuePair<string, string?>("key1", "value1")]);
        var cancellationTokenSource = new CancellationTokenSource();

        using var ms = new MemoryStream();
        await JsonSerializer.SerializeAsync(ms, responseMessage, cancellationToken: cancellationTokenSource.Token);
        var responseData = (ReadOnlyMemory<byte>)ms.ToArray();

        var queryResult = new ServiceQueryResult(Guid.NewGuid().ToString(), new MessageHeader([]), "U-BasicResponseMessage-0.0.0.0", responseData);

        List<ServiceMessage> messages = [];
        List<TimeSpan> timeouts = [];

        var serviceConnection = new Mock<IQueryResponseMessageServiceConnection>();
        serviceConnection.Setup(x => x.QueryAsync(Capture.In(messages), Capture.In(timeouts), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        var contractConnection = ContractConnection.MultiServiceInstance()
            .RegisterServiceConnection(ServiceName, serviceConnection.Object);
        #endregion

        #region Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result1 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, timeout: timeout, channel: channel, responseChannel: responseChannel, messageHeader: headers, cancellationToken: cancellationTokenSource.Token);
        var result2 = await contractConnection.QueryAsync<BasicQueryMessage>(testMessage, timeout: timeout, channel: channel, responseChannel: responseChannel, messageHeader: headers, cancellationToken: cancellationTokenSource.Token);
#pragma warning restore CS0618 // Type or member is obsolete
        var result3 = await contractConnection.QueryAsync<BasicQueryMessage, BasicResponseMessage>(testMessage, timeout: timeout, channel: channel, responseChannel: responseChannel, cancellationToken: cancellationTokenSource.Token);
        var result4 = await contractConnection.QueryAsync<BasicQueryMessage>(testMessage, timeout: timeout, channel: channel, responseChannel: responseChannel, cancellationToken: cancellationTokenSource.Token);
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, 4, TimeSpan.FromMinutes(1)));
        Assert.IsTrue(result1.Concat(result3).All(r => r != null && r.ID == queryResult.ID && r.Error == null && !r.IsError));
        Assert.IsTrue(result2.Concat(result4).All(r => r != null && r.ID == queryResult.ID && r.Error == null && !r.IsError));
        Assert.IsTrue(messages.All(m => Equals(m.Channel, channel)));
        Assert.HasCount(4, timeouts);
        Assert.IsTrue(timeouts.All(t => t == timeout));
        Assert.IsTrue(messages[0].Header.AsEnumerable().SequenceEqual(headers.AsEnumerable()));
        Assert.IsTrue(messages[1].Header.AsEnumerable().SequenceEqual(headers.AsEnumerable()));
        Assert.AreEqual(0, messages[2].Header.Count);
        Assert.AreEqual(0, messages[3].Header.Count);
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[0].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[1].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[2].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        Assert.AreEqual(testMessage, await JsonSerializer.DeserializeAsync<BasicQueryMessage>(new MemoryStream(messages[3].Data.ToArray()), cancellationToken: cancellationTokenSource.Token));
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.QueryAsync(It.Is<ServiceMessage>(sm => sm.Channel == channel && sm.Header.AsEnumerable().SequenceEqual(headers.AsEnumerable())), timeout, cancellationTokenSource.Token), Times.Exactly(2));
        serviceConnection.Verify(x => x.QueryAsync(It.Is<ServiceMessage>(sm => sm.Channel == channel && sm.Header.Count==0), timeout, cancellationTokenSource.Token), Times.Exactly(2));
        serviceConnection.Verify(x => x.QueryAsync(It.IsAny<ServiceMessage>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
        #endregion
    }
}
