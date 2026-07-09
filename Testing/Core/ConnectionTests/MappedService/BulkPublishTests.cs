using CoreTesting.Messages;
using Moq;
using MQContract;
using MQContract.Attributes;
using MQContract.Interfaces.Service;
using Polly.CircuitBreaker;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace CoreTesting.ConnectionTests.MappedService;

[TestClass]
public class BulkPublishTests
{
    private const string ServiceName = "testService";

    [TestMethod]
    public async Task TestBulkPublishAsync()
    {
        #region Arrange
        var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

        IEnumerable<TransmissionMessage<BasicMessage>> testMessages = [
            new(new("testMessage")),
            new(new("testMessage2"))
        ];

        List<IEnumerable<ServiceMessage>> messages = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.BulkPublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
            .Returns((IEnumerable<ServiceMessage> ms, CancellationToken ct) => ValueTask.FromResult(ms.Select(m => transmissionResult)));

        var contractConnection = ContractConnection.MappedServiceInstance()
            .RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
        #endregion

        #region Act
        var stopwatch = Stopwatch.StartNew();
        var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, cancellationToken: TestContext.CancellationToken);
        stopwatch.Stop();
        Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(result);
        Assert.IsTrue(result.All(r => Equals(r, transmissionResult)));
        Assert.HasCount(1, messages);
        Assert.IsTrue(messages.SelectMany(messageSet => messageSet.Select(m => m)).All(m =>
            Equals(typeof(BasicMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, m.Channel)
            && Equals(0, m.Header.Keys.Count())
            && Equals(Constants.BasicMessageType, m.MessageTypeID)
            && m.Data.Length > 0
        ));
        Assert.AreEqual(testMessages.ElementAt(0).Message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].ElementAt(0).Data.ToArray()), cancellationToken: TestContext.CancellationToken));
        Assert.AreEqual(testMessages.ElementAt(1).Message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].ElementAt(1).Data.ToArray()), cancellationToken: TestContext.CancellationToken));
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
        #endregion
    }

    [TestMethod]
    public async Task TestBulkPublishAsyncWithCustomChannelAndHeadersAndIDs()
    {
        #region Arrange
        var transmissionResult = new TransmissionResult(Guid.NewGuid().ToString());

        var channel = "customChannel";
        IEnumerable<TransmissionMessage<BasicMessage>> testMessages = [
            new(new("testMessage"), Header:new([new("key1","value1")]), ID: Guid.NewGuid().ToString()),
            new(new("testMessage2"), Header:new([new("key1","value1")]), ID: Guid.NewGuid().ToString())
        ];

        List<IEnumerable<ServiceMessage>> messages = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.BulkPublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
            .Returns((IEnumerable<ServiceMessage> ms, CancellationToken ct) => ValueTask.FromResult(ms.Select(m => transmissionResult)));

        var contractConnection = ContractConnection.MappedServiceInstance()
            .RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
        #endregion

        #region Act
        var stopwatch = Stopwatch.StartNew();
        var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel, cancellationToken: TestContext.CancellationToken);
        stopwatch.Stop();
        Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(result);
        Assert.IsTrue(result.All(r => Equals(r, transmissionResult)));
        Assert.HasCount(1, messages);
        Assert.IsTrue(messages.SelectMany(messageSet => messageSet.Select(m => m)).All(m =>
            Equals(channel, m.Channel)
            && Equals(1, m.Header.Keys.Count())
            && Equals(Constants.BasicMessageType, m.MessageTypeID)
            && m.Data.Length > 0
        ));
        Assert.AreEqual(testMessages.ElementAt(0).Message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].ElementAt(0).Data.ToArray()), cancellationToken: TestContext.CancellationToken));
        Assert.AreEqual(testMessages.ElementAt(1).Message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].ElementAt(1).Data.ToArray()), cancellationToken: TestContext.CancellationToken));
        Assert.AreEqual(testMessages.ElementAt(0).Header!.Keys.First(), messages[0].ElementAt(0).Header.Keys.First());
        Assert.AreEqual(testMessages.ElementAt(0).Header![testMessages.ElementAt(0).Header!.Keys.First()], messages[0].ElementAt(0).Header[messages[0].ElementAt(0).Header.Keys.First()]);
        Assert.AreEqual(testMessages.ElementAt(1).Header!.Keys.First(), messages[0].ElementAt(1).Header.Keys.First());
        Assert.AreEqual(testMessages.ElementAt(1).Header![testMessages.ElementAt(1).Header!.Keys.First()], messages[0].ElementAt(1).Header[messages[0].ElementAt(1).Header.Keys.First()]);
        Assert.AreEqual(testMessages.ElementAt(0).ID, messages[0].ElementAt(0).ID);
        Assert.AreEqual(testMessages.ElementAt(1).ID, messages[0].ElementAt(1).ID);
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
        #endregion
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task TestBulkPublishAsyncWithTelemetryData(bool withLinking)
    {
        #region Arrange
        (var listener, var capturedActivities, var sourceName) = ConnectionHelper.SetupTelemetry();

        IEnumerable<TransmissionMessage<BasicMessage>> testMessages = [
            new(new("testMessage")),
            new(new("testMessage2"))
        ];

        List<IEnumerable<ServiceMessage>> messages = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.BulkPublishAsync(Capture.In(messages), It.IsAny<CancellationToken>()))
            .Returns((IEnumerable<ServiceMessage> ms, CancellationToken ct) => ValueTask.FromResult(ms.Select(m => new TransmissionResult(m.ID))));

        var contractConnection = ContractConnection.MappedServiceInstance()
            .RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object)
            .EnableOpenTelemetry(activitySource: sourceName, linkActivitiesAcrossSystems: withLinking);
        #endregion

        #region Act
        var stopwatch = Stopwatch.StartNew();
        var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, cancellationToken: TestContext.CancellationToken);
        stopwatch.Stop();
        Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, 1, TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(result);
        Assert.HasCount(1, messages);
        Assert.IsTrue(messages.SelectMany(messageSet => messageSet.Select(m => m)).All(m =>
            Equals(typeof(BasicMessage).GetCustomAttribute<MessageAttribute>(false)?.Channel, m.Channel)
            && Equals((withLinking ? 2 : 0), m.Header.Keys.Count())
            && Equals(Constants.BasicMessageType, m.MessageTypeID)
            && m.Data.Length > 0
        ));
        Assert.AreEqual(testMessages.ElementAt(0).Message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].ElementAt(0).Data.ToArray()), cancellationToken: TestContext.CancellationToken));
        Assert.AreEqual(testMessages.ElementAt(1).Message, await JsonSerializer.DeserializeAsync<BasicMessage>(new MemoryStream(messages[0].ElementAt(1).Data.ToArray()), cancellationToken: TestContext.CancellationToken));
        Assert.HasCount(1, capturedActivities);
        ConnectionHelper.ValidateBulkPublishActivity<BasicMessage>(
            messages.SelectMany(m => m),
            capturedActivities[0],
            "MQContract.BulkPublishMessages",
            serviceConnection.Object.GetType(),
            true,
            withLinking,
            connectionName: ServiceName
        );
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
        #endregion
    }

    [TestMethod]
    [DataRow(null, null, false)]
    [DataRow(null, null, true)]
    [DataRow("testChannel", null, false)]
    [DataRow(null, typeof(BasicMessage), false)]
    public async Task TestBulkPublishAsyncWithRetryFailure(string? channel, Type? messageType, bool useGenerics)
    {
        #region Arrange
        var error = new Exception("Failed");
        var retryCount = 2;

        IEnumerable<TransmissionMessage<BasicMessage>> testMessages = [
            new(new("testMessage")),
            new(new("testMessage2"))
        ];

        List<IEnumerable<ServiceMessage>> messages = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()))
            .Returns((IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken) =>
            {
                messages.Add(serviceMessages);
                if (serviceMessages.Count() == 2)
                    return ValueTask.FromResult<IEnumerable<TransmissionResult>>([new(serviceMessages.First().ID), new(serviceMessages.Last().ID, new(error, false))]);
                return ValueTask.FromResult(serviceMessages.Select(m => new TransmissionResult(m.ID, new(error, false))));
            });

        var contractConnection = ContractConnection.MappedServiceInstance()
            .RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
        ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, null);
        #endregion

        #region Act
        var stopwatch = Stopwatch.StartNew();
        var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel, cancellationToken: TestContext.CancellationToken);
        stopwatch.Stop();
        Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, retryCount + 1, TimeSpan.FromMinutes(1)));
        Assert.HasCount(2, messages[0]);
        Assert.IsTrue(Array.TrueForAll(messages.Skip(1).ToArray(), (msgs) => msgs.Count() == 1));
        Assert.IsNotNull(result);
        Assert.HasCount(testMessages.Count(), result);
        var failed = result.Last();
        Assert.IsTrue(failed.IsError);
        Assert.IsNotNull(failed.Error);
        Assert.IsInstanceOfType<ResilienceException>(failed.Error.Exception);
        Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)failed.Error.Exception).Type);
        Assert.IsNotNull(failed.Error.Exception.InnerException);
        Assert.AreEqual(error.Message, failed.Error.Exception.InnerException.Message);
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount + 1));
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        #endregion
    }

    [TestMethod]
    [DataRow(null, null, false)]
    [DataRow(null, null, true)]
    [DataRow("testChannel", null, false)]
    [DataRow(null, typeof(BasicMessage), false)]
    public async Task TestBulkPublishAsyncWithCircuitBreakFailure(string? channel, Type? messageType, bool useGenerics)
    {
        #region Arrange
        var error = new Exception("Failed");
        var circuitBreakCount = 1;

        IEnumerable<TransmissionMessage<BasicMessage>> testMessages = [
            new(new("testMessage")),
            new(new("testMessage2"))
        ];

        List<IEnumerable<ServiceMessage>> messages = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()))
            .Returns((IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken) =>
            {
                messages.Add(serviceMessages);
                if (serviceMessages.Count() == 2)
                    return ValueTask.FromResult<IEnumerable<TransmissionResult>>([new(serviceMessages.First().ID), new(serviceMessages.Last().ID, new(error, false))]);
                return ValueTask.FromResult(serviceMessages.Select(m => new TransmissionResult(m.ID, new(error, false))));
            });

        var contractConnection = ContractConnection.MappedServiceInstance()
            .RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
        ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, null, circuitBreakCount);
        #endregion

        #region Act
        var stopwatch = Stopwatch.StartNew();
        _ = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel, cancellationToken: TestContext.CancellationToken);
        var result = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel, cancellationToken: TestContext.CancellationToken);
        stopwatch.Stop();
        Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, circuitBreakCount, TimeSpan.FromMinutes(1)));
        Assert.HasCount(2, messages[0]);
        var failed = result.Last();
        Assert.IsTrue(failed.IsError);
        Assert.IsNotNull(failed.Error);
        Assert.IsInstanceOfType<ResilienceException>(failed.Error.Exception);
        Assert.AreEqual(ResilienceTypes.CircuitBreak, ((ResilienceException)failed.Error.Exception).Type);
        Assert.IsNotNull(failed.Error.Exception.InnerException);
        Assert.IsInstanceOfType<BrokenCircuitException>(failed.Error.Exception.InnerException);
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(circuitBreakCount));
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        #endregion
    }

    [TestMethod]
    [DataRow(null, null, false)]
    [DataRow(null, null, true)]
    [DataRow("testChannel", null, false)]
    [DataRow(null, typeof(BasicMessage), false)]
    public async Task TestBulkPublishAsyncWithRetryAndCircuitBreakFailure(string? channel, Type? messageType, bool useGenerics)
    {
        #region Arrange
        var error = new Exception("Failed");
        var circuitBreakCount = 3;
        var retryCount = 2;

        IEnumerable<TransmissionMessage<BasicMessage>> testMessages = [
            new(new("testMessage")),
            new(new("testMessage2"))
        ];

        List<IEnumerable<ServiceMessage>> messages = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()))
            .Returns((IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken) =>
            {
                messages.Add(serviceMessages);
                if (serviceMessages.Count() == 2)
                    return ValueTask.FromResult<IEnumerable<TransmissionResult>>([new(serviceMessages.First().ID), new(serviceMessages.Last().ID, new(error, false))]);
                return ValueTask.FromResult(serviceMessages.Select(m => new TransmissionResult(m.ID, new(error, false))));
            });

        var contractConnection = ContractConnection.MappedServiceInstance()
            .RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
        ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
        #endregion

        #region Act
        var stopwatch = Stopwatch.StartNew();
        var retryResults = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel, cancellationToken: TestContext.CancellationToken);
        var circuitBreakResults = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel, cancellationToken: TestContext.CancellationToken);
        stopwatch.Stop();
        Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, circuitBreakCount, TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(retryResults);
        Assert.IsNotNull(circuitBreakResults);
        Assert.HasCount(testMessages.Count(), retryResults);
        Assert.HasCount(testMessages.Count(), circuitBreakResults);
        var retryFailure = retryResults.Last();
        Assert.IsTrue(retryFailure.IsError);
        Assert.IsNotNull(retryFailure.Error);
        Assert.IsInstanceOfType<ResilienceException>(retryFailure.Error.Exception);
        Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)retryFailure.Error.Exception).Type);
        Assert.AreEqual(error, retryFailure.Error.Exception.InnerException);
        Assert.IsTrue(Array.TrueForAll(circuitBreakResults.ToArray(), result => result.IsError
        && result.Error != null
        && result.Error.Exception is ResilienceException re
        && Equals(ResilienceTypes.CircuitBreak, re.Type)
        && re.InnerException is BrokenCircuitException));
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount + 1));
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        #endregion
    }

    [TestMethod]
    [DataRow(null, null, false)]
    [DataRow(null, null, true)]
    [DataRow("testChannel", null, false)]
    [DataRow(null, typeof(BasicMessage), false)]
    public async Task TestBulkPublishAsyncWithRetryAndCircuitBreakWithoutFailure(string? channel, Type? messageType, bool useGenerics)
    {
        #region Arrange
        var error = new Exception("Failed");
        var circuitBreakCount = 3;
        var retryCount = 2;

        IEnumerable<TransmissionMessage<BasicMessage>> testMessages = [
            new(new("testMessage")),
            new(new("testMessage2"))
        ];

        List<IEnumerable<ServiceMessage>> messages = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()))
            .Returns((IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken) =>
            {
                messages.Add(serviceMessages);
                return ValueTask.FromResult(serviceMessages.Select(m => new TransmissionResult(m.ID, new(error, true))));
            });

        var contractConnection = ContractConnection.MappedServiceInstance()
            .RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
        ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, messageType, useGenerics, retryCount, circuitBreakCount);
        #endregion

        #region Act
        var stopwatch = Stopwatch.StartNew();
        var retryResults = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel, cancellationToken: TestContext.CancellationToken);
        var circuitBreakResults = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel, cancellationToken: TestContext.CancellationToken);
        stopwatch.Stop();
        Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, testMessages.Count(), TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(retryResults);
        Assert.HasCount(testMessages.Count(), retryResults);
        Assert.IsTrue(Array.TrueForAll(retryResults.ToArray(), r => r.IsError && Equals(error, r.Error!.Exception) && r.Error!.IsFatal));
        Assert.IsNotNull(circuitBreakResults);
        Assert.HasCount(testMessages.Count(), circuitBreakResults);
        Assert.IsTrue(Array.TrueForAll(circuitBreakResults.ToArray(), r => r.IsError && Equals(error, r.Error!.Exception) && r.Error!.IsFatal));
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        #endregion
    }

    [TestMethod()]
    public async Task TestBulkPublishAsyncWithRetryAndCircuitBreakFailureAndEnsuringPriority()
    {
        #region Arrange
        var channel = "testChannel";
        var messageType = typeof(BasicMessage);
        var error = new Exception("Failed");
        var circuitBreakCount = 2;
        var retryCount = 1;

        IEnumerable<TransmissionMessage<BasicMessage>> testMessages = [
            new(new("testMessage")),
            new(new("testMessage2"))
        ];

        List<IEnumerable<ServiceMessage>> messages = [];

        var serviceConnection = new Mock<IMessageServiceConnection>();
        serviceConnection.Setup(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()))
            .Returns((IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken) =>
            {
                messages.Add(serviceMessages);
                if (serviceMessages.Count() == 2)
                    return ValueTask.FromResult<IEnumerable<TransmissionResult>>([new(serviceMessages.First().ID), new(serviceMessages.Last().ID, new(error, false))]);
                return ValueTask.FromResult(serviceMessages.Select(m => new TransmissionResult(m.ID, new(error, false))));
            });

        var contractConnection = ContractConnection.MappedServiceInstance()
            .RegisterServiceConnection((props) => true, ServiceName, serviceConnection.Object);
        ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, channel, null, false, retryCount, circuitBreakCount);
        ConnectionHelper.AssignResiliencePolicy<BasicMessage>(contractConnection, ServiceName, null, messageType, false, retryCount + 1, circuitBreakCount + 1);
        #endregion

        #region Act
        var stopwatch = Stopwatch.StartNew();
        var channelRetryResults = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel, cancellationToken: TestContext.CancellationToken);
        var channelCircuitBreakResults = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, channel: channel, cancellationToken: TestContext.CancellationToken);
        var typeRetryResults = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, cancellationToken: TestContext.CancellationToken);
        var typeCircuitBreakResults = await contractConnection.BulkPublishAsync<BasicMessage>(testMessages, cancellationToken: TestContext.CancellationToken);
        stopwatch.Stop();
        Trace.WriteLine($"Time to publish message {stopwatch.ElapsedMilliseconds}ms");
        #endregion

        #region Assert
        Assert.IsTrue(await Helper.WaitForCount(messages, (circuitBreakCount * 2) + 1, TimeSpan.FromMinutes(1)));
        Assert.IsNotNull(channelRetryResults);
        Assert.IsNotNull(channelCircuitBreakResults);
        Assert.HasCount(testMessages.Count(), channelRetryResults);
        Assert.HasCount(testMessages.Count(), channelCircuitBreakResults);
        var retryFailure = channelRetryResults.Last();
        Assert.IsTrue(retryFailure.IsError);
        Assert.IsNotNull(retryFailure.Error);
        Assert.IsInstanceOfType<ResilienceException>(retryFailure.Error.Exception);
        Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)retryFailure.Error.Exception).Type);
        Assert.AreEqual(error, retryFailure.Error.Exception.InnerException);
        Assert.IsTrue(Array.TrueForAll(channelCircuitBreakResults.ToArray(), result => result.IsError
        && result.Error != null
        && result.Error.Exception is ResilienceException re
        && Equals(ResilienceTypes.CircuitBreak, re.Type)
        && re.InnerException is BrokenCircuitException));
        Assert.IsNotNull(typeRetryResults);
        Assert.IsNotNull(typeCircuitBreakResults);
        Assert.HasCount(testMessages.Count(), typeRetryResults);
        Assert.HasCount(testMessages.Count(), typeCircuitBreakResults);
        retryFailure = typeRetryResults.Last();
        Assert.IsTrue(retryFailure.IsError);
        Assert.IsNotNull(retryFailure.Error);
        Assert.IsInstanceOfType<ResilienceException>(retryFailure.Error.Exception);
        Assert.AreEqual(ResilienceTypes.Retry, ((ResilienceException)retryFailure.Error.Exception).Type);
        Assert.AreEqual(error, retryFailure.Error.Exception.InnerException);
        Assert.IsTrue(Array.TrueForAll(typeCircuitBreakResults.ToArray(), result => result.IsError
        && result.Error != null
        && result.Error.Exception is ResilienceException re2
        && Equals(ResilienceTypes.CircuitBreak, re2.Type)
        && re2.InnerException is BrokenCircuitException));
        #endregion

        #region Verify
        serviceConnection.Verify(x => x.BulkPublishAsync(It.IsAny<IEnumerable<ServiceMessage>>(), It.IsAny<CancellationToken>()), Times.Exactly(((retryCount + 1) * 2) + 1));
        serviceConnection.Verify(x => x.PublishAsync(It.IsAny<ServiceMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        #endregion
    }

    public TestContext TestContext { get; set; }
}
