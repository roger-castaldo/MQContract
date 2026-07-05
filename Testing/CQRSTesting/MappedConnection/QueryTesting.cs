using CQRSTesting.Messages;
using Moq;
using MQContract;
using MQContract.CQRS;
using MQContract.CQRS.Extensions;
using MQContract.CQRS.Interfaces;
using MQContract.CQRS.Interfaces.Query;
using MQContract.Interfaces;
using MQContract.Messages;


namespace CQRSTesting.MappedConnection
{
    [TestClass]
    public class QueryTesting
    {
        private const string groupName = "QueryWithResponseTesting";

        [TestMethod]
        public async Task TestSimpleQuery()
        {
            #region Arrange
            var receivedQuerys = new List<BasicQuery>();

            var mockQueryProcessor = new Mock<IQueryProcessor<BasicQuery, BasicQueryResponse>>();

            mockQueryProcessor.Setup(x => x.ProcessQueryAsync(It.IsAny<IQueryInvocationContext<BasicQuery>>(), It.IsAny<CancellationToken>()))
                .Returns((IQueryInvocationContext<BasicQuery> context, CancellationToken cancellationToken) =>
                {
                    receivedQuerys.Add(context.Query);
                    return ValueTask.FromResult<BasicQueryResponse>(new(context.Query.Name));
                });
            mockQueryProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(true);
            var cqrsConnection = await contractConnection.CreateCQRSConnection()
                .RegisterQueryProcessorAsync<BasicQuery, BasicQueryResponse>(mockQueryProcessor.Object);

            var command = new BasicQuery(Helper.GenerateRandomString());
            #endregion

            #region Act
            var result = await cqrsConnection.ExecuteQueryAsync<BasicQuery, BasicQueryResponse>(command, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(receivedQuerys, 1, TimeSpan.FromMinutes(1)));
            Assert.HasCount(1, receivedQuerys);
            Assert.AreEqual(command, receivedQuerys[0]);
            Assert.IsNotNull(result);
            Assert.AreEqual(command.Name, result.Name);
            #endregion

            #region Verify
            mockQueryProcessor.Verify(x => x.ProcessQueryAsync(It.IsAny<IQueryInvocationContext<BasicQuery>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockQueryProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestSimpleQueryWithContext()
        {
            #region Arrange
            var receivedContexts = new List<IQueryInvocationContext<BasicQuery>>();

            var mockQueryProcessor = new Mock<IQueryProcessor<BasicQuery, BasicQueryResponse>>();

            mockQueryProcessor.Setup(x => x.ProcessQueryAsync(It.IsAny<IQueryInvocationContext<BasicQuery>>(), It.IsAny<CancellationToken>()))
                .Returns((IQueryInvocationContext<BasicQuery> context, CancellationToken cancellationToken) =>
                {
                    receivedContexts.Add(context);
                    return ValueTask.FromResult<BasicQueryResponse>(new(context.Query.Name));
                });
            mockQueryProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(true);
            var cqrsConnection = await contractConnection.CreateCQRSConnection()
                .RegisterQueryProcessorAsync<BasicQuery, BasicQueryResponse>(mockQueryProcessor.Object, group: groupName);

            var command = new BasicQuery(Helper.GenerateRandomString());
            var context = new Context();
            context[Helper.GenerateRandomString()] = Helper.GenerateRandomString();
            #endregion

            #region Act
            var result = await cqrsConnection.ExecuteQueryAsync<BasicQuery, BasicQueryResponse>(command, context: context, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(receivedContexts, 1, TimeSpan.FromMinutes(1)));
            Assert.HasCount(1, receivedContexts);
            Assert.AreEqual(command, receivedContexts[0].Query);
            Assert.HasCount(1, receivedContexts[0].Keys);
            Assert.IsNull(receivedContexts[0].CausationId);
            Assert.AreEqual(context.CorrelationId, receivedContexts[0].CorrelationId);
            Assert.AreEqual(context.MessageId, receivedContexts[0].MessageId);
            Assert.AreEqual(context[context.Keys.First()], receivedContexts[0][receivedContexts[0].Keys.First()]);
            Assert.IsNotNull(result);
            Assert.AreEqual(command.Name, result.Name);
            #endregion

            #region Verify
            mockQueryProcessor.Verify(x => x.ProcessQueryAsync(It.IsAny<IQueryInvocationContext<BasicQuery>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockQueryProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestCancellationTokenCallThroughQuery()
        {
            #region Arrange
            var receivedQuerys = new List<BasicQuery>();
            var cancellationTokenSource = new CancellationTokenSource();

            var mockQueryProcessor = new Mock<IQueryProcessor<BasicQuery, BasicQueryResponse>>();

            mockQueryProcessor.Setup(x => x.ProcessQueryAsync(It.IsAny<IQueryInvocationContext<BasicQuery>>(), It.IsAny<CancellationToken>()))
                .Returns(async (IQueryInvocationContext<BasicQuery> context, CancellationToken cancellationToken) =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), TestContext.CancellationToken).ConfigureAwait(false);
                    if (!cancellationToken.IsCancellationRequested)
                        receivedQuerys.Add(context.Query);
                    return new(context.Query.Name);
                });
            mockQueryProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(true);
            var cqrsConnection = await contractConnection.CreateCQRSConnection("cancelCalls")
                .RegisterQueryProcessorAsync<BasicQuery, BasicQueryResponse>(mockQueryProcessor.Object, group: groupName);

            var command = new BasicQuery(Helper.GenerateRandomString());
            #endregion

            #region Act
            _ = Task.Delay(TimeSpan.FromSeconds(1), TestContext.CancellationToken)
                .ContinueWith((tsk) => cancellationTokenSource.Cancel(), TestContext.CancellationToken);
            var err = await Assert.ThrowsAsync<QueryCallException>(async () => await cqrsConnection.ExecuteQueryAsync<BasicQuery, BasicQueryResponse>(command, cancellationToken: cancellationTokenSource.Token));
            #endregion

            #region Assert
            Assert.HasCount(0, receivedQuerys);
            Assert.IsNotNull(err);
            Assert.IsInstanceOfType<TaskCanceledException>(err.Error.Exception);
            #endregion

            #region Verify
            mockQueryProcessor.Verify(x => x.ProcessQueryAsync(It.IsAny<IQueryInvocationContext<BasicQuery>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockQueryProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.AtMostOnce());
            #endregion
        }

        [TestMethod]
        public async Task TestQueryContextSharing()
        {
            #region Arrange
            var isSecondCallHeader = "isSecondCall";
            var receivedContexts = new List<IQueryInvocationContext<BasicQuery>>();

            var command = new BasicQuery(Helper.GenerateRandomString());
            var secondQuery = new BasicQuery(Helper.GenerateRandomString());

            var mockQueryProcessor = new Mock<IQueryProcessor<BasicQuery, BasicQueryResponse>>();

            mockQueryProcessor.Setup(x => x.ProcessQueryAsync(It.IsAny<IQueryInvocationContext<BasicQuery>>(), It.IsAny<CancellationToken>()))
                .Returns(async (IQueryInvocationContext<BasicQuery> context, CancellationToken cancellationToken) =>
                {
                    receivedContexts.Add(context);
                    if (string.IsNullOrEmpty(context[isSecondCallHeader]))
                    {
                        context[isSecondCallHeader] = "true";
                        return (await context.ExecuteQueryAsync<BasicQuery, BasicQueryResponse>(secondQuery))!;
                    }
                    return new BasicQueryResponse(context.Query.Name);
                });
            mockQueryProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(true);
            var cqrsConnection = await contractConnection.CreateCQRSConnection()
                .RegisterQueryProcessorAsync<BasicQuery, BasicQueryResponse>(mockQueryProcessor.Object, group: groupName)
                .RegisterQueryProcessorAsync<BasicQuery, BasicQueryResponse>(mockQueryProcessor.Object, group: groupName);


            var context = new Context();
            #endregion

            #region Act
            var result = await cqrsConnection.ExecuteQueryAsync<BasicQuery, BasicQueryResponse>(command, context: context, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(receivedContexts, 2, TimeSpan.FromMinutes(1)));
            Assert.HasCount(2, receivedContexts);
            Assert.AreEqual(command, receivedContexts[0].Query);
            Assert.HasCount(1, receivedContexts[0].Keys);
            Assert.IsNull(receivedContexts[0].CausationId);
            Assert.AreEqual(context.CorrelationId, receivedContexts[0].CorrelationId);
            Assert.AreEqual(context.MessageId, receivedContexts[0].MessageId);
            Assert.HasCount(1, receivedContexts[1].Keys);
            Assert.AreEqual(context.CorrelationId, receivedContexts[1].CorrelationId);
            Assert.AreEqual(context.MessageId, receivedContexts[1].CausationId);
            Assert.AreNotEqual(context.MessageId, receivedContexts[1].MessageId);
            Assert.AreEqual(secondQuery, receivedContexts[1].Query);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Name, secondQuery.Name);
            #endregion

            #region Verify
            mockQueryProcessor.Verify(x => x.ProcessQueryAsync(It.IsAny<IQueryInvocationContext<BasicQuery>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockQueryProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestCancellationTokenCallThroughChainedQuery()
        {
            #region Arrange
            var isSecondCallHeader = "isSecondCall";
            var receivedQuerys = new List<BasicQuery>();
            var cancellationTokenSource = new CancellationTokenSource();

            var command = new BasicQuery(Helper.GenerateRandomString());
            var secondQuery = new BasicQuery(Helper.GenerateRandomString());

            var mockQueryProcessor = new Mock<IQueryProcessor<BasicQuery, BasicQueryResponse>>();

            mockQueryProcessor.Setup(x => x.ProcessQueryAsync(It.IsAny<IQueryInvocationContext<BasicQuery>>(), It.IsAny<CancellationToken>()))
                .Returns(async (IQueryInvocationContext<BasicQuery> context, CancellationToken cancellationToken) =>
                {
                    if (string.IsNullOrEmpty(context[isSecondCallHeader]))
                    {
                        receivedQuerys.Add(context.Query);
                        context[isSecondCallHeader] = "true";
                        return (await context.ExecuteQueryAsync<BasicQuery, BasicQueryResponse>(secondQuery))!;
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2), TestContext.CancellationToken).ConfigureAwait(false);
                        if (!cancellationToken.IsCancellationRequested)
                            receivedQuerys.Add(context.Query);
                        return new BasicQueryResponse(context.Query.Name);
                    }
                });
            mockQueryProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(true);
            var cqrsConnection = await contractConnection.CreateCQRSConnection("cancelCalls")
                .RegisterQueryProcessorAsync<BasicQuery, BasicQueryResponse>(mockQueryProcessor.Object, group: groupName)
                .RegisterQueryProcessorAsync<BasicQuery, BasicQueryResponse>(mockQueryProcessor.Object, group: groupName);


            var context = new Context();
            #endregion

            #region Act
            _ = Task.Delay(TimeSpan.FromSeconds(1), TestContext.CancellationToken)
                .ContinueWith((tsk) => cancellationTokenSource.Cancel(), TestContext.CancellationToken);
            var err = await Assert.ThrowsAsync<QueryCallException>(async () => await cqrsConnection.ExecuteQueryAsync<BasicQuery, BasicQueryResponse>(command, cancellationToken: cancellationTokenSource.Token));
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(receivedQuerys, 1, TimeSpan.FromMinutes(1)));
            Assert.AreEqual(command, receivedQuerys[0]);
            Assert.IsNotNull(err);
            Assert.IsInstanceOfType<TaskCanceledException>(err.Error.Exception);
            #endregion

            #region Verify
            mockQueryProcessor.Verify(x => x.ProcessQueryAsync(It.IsAny<IQueryInvocationContext<BasicQuery>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockQueryProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.AtMostOnce());
            #endregion
        }

        [TestMethod]
        [DataRow("headerValue", "differentHeaderValue", "messageHeaderValue", "messageHeaderValue", "messageValue", "messageValue")]
        [DataRow("headerValue", "headerValue", "messageHeaderValue", "differentMessageHeaderValue", "messageValue", "messageValue")]
        [DataRow("headerValue", "headerValue", "messageHeaderValue", "messageHeaderValue", "messageValue", "differentMessageValue")]
        [DataRow("headerValue", "headerValue", "messageHeaderValue", "messageHeaderValue", "messageValue", "messageValue")]
        public async Task TestQueryProcessorWithFiltering(string headerValue, string checkValue, string messageHeaderValue, string messageHeaderCheckValue,
            string messageValue, string messageCheckValue)
        {
            #region Arrange
            var headerKey = "testHeader";
            var messageHeaderKey = "testMessageHeader";
            var receivedQuerys = new List<BasicQuery>();
            var dropped = false;

            var mockQueryProcessor = new Mock<IQueryProcessor<BasicQuery, BasicQueryResponse>>();

            mockQueryProcessor.Setup(x => x.ProcessQueryAsync(It.IsAny<IQueryInvocationContext<BasicQuery>>(), It.IsAny<CancellationToken>()))
                .Returns((IQueryInvocationContext<BasicQuery> context, CancellationToken cancellationToken) =>
                {
                    receivedQuerys.Add(context.Query);
                    return ValueTask.FromResult<BasicQueryResponse>(new(context.Query.Name));
                });
            mockQueryProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            if (!Equals(headerValue, checkValue))
                mockQueryProcessor.As<IContextFilteredProcessor>().SetupGet(x => x.Filter)
                    .Returns((Context context) =>
                    {
                        if (!Equals(context[headerKey], checkValue))
                        {
                            dropped=true;
                            return ValueTask.FromResult<MessageFilterResult>(MessageFilterResult.DropAndAcknowledge);
                        }
                        return ValueTask.FromResult<MessageFilterResult>(MessageFilterResult.Allow);
                    });
            if (Equals(headerValue, checkValue))
                mockQueryProcessor.As<IFilteredQueryProcessor<BasicQuery, BasicQueryResponse>>().SetupGet(x => x.Filter)
                    .Returns((BasicQuery command, Context context) =>
                    {
                        (var isDropped, var result) = (Equals(context[messageHeaderKey], messageHeaderCheckValue), Equals(command.Name, messageCheckValue)) switch
                        {
                            (true, true) => (false, MessageFilterResult.Allow),
                            (false, _) => (true, MessageFilterResult.DropAndAcknowledge),
                            (_, false) => (true, MessageFilterResult.DropAndDontAcknowledge)
                        };
                        dropped = isDropped;
                        return ValueTask.FromResult(result);
                    });

            await using var contractConnection = Helper.ProduceConnection(true);
            var cqrsConnection = await contractConnection.CreateCQRSConnection()
                .RegisterQueryProcessorAsync<BasicQuery, BasicQueryResponse>(mockQueryProcessor.Object);

            var command = new BasicQuery(messageValue);
            var context = new Context();
            context[headerKey] = headerValue;
            context[messageHeaderKey] = messageHeaderValue;

            BasicQueryResponse? result = null;
            Exception? error = null;
            #endregion

            #region Act
            if (Equals(headerValue, checkValue) && Equals(messageHeaderValue, messageHeaderCheckValue) && Equals(messageValue, messageCheckValue))
                result = await cqrsConnection.ExecuteQueryAsync<BasicQuery, BasicQueryResponse>(command, context: context, cancellationToken: TestContext.CancellationToken);
            else
                error = await Assert.ThrowsAsync<QueryTimeoutException>(async () => await cqrsConnection.ExecuteQueryAsync<BasicQuery, BasicQueryResponse>(command, context: context, timeout: TimeSpan.FromMilliseconds(500), TestContext.CancellationToken));
            #endregion

            #region Assert
            if (Equals(headerValue, checkValue) && Equals(messageHeaderValue, messageHeaderCheckValue) && Equals(messageValue, messageCheckValue))
            {
                Assert.IsTrue(await Helper.WaitForCount(receivedQuerys, 1, TimeSpan.FromMinutes(1)));
                Assert.HasCount(1, receivedQuerys);
                Assert.AreEqual(command, receivedQuerys[0]);
                Assert.IsFalse(dropped);
                Assert.IsNull(error);
                Assert.IsNotNull(result);
                Assert.AreEqual(command.Name, result.Name);
                mockQueryProcessor.Verify(x => x.ProcessQueryAsync(It.IsAny<IQueryInvocationContext<BasicQuery>>(), It.IsAny<CancellationToken>()), Times.Once);
            }
            else
            {
                Assert.IsEmpty(receivedQuerys);
                Assert.IsTrue(dropped);
                Assert.IsNotNull(error);
                Assert.IsInstanceOfType<QueryTimeoutException>(error);
                Assert.IsNull(result);
                mockQueryProcessor.Verify(x => x.ProcessQueryAsync(It.IsAny<IQueryInvocationContext<BasicQuery>>(), It.IsAny<CancellationToken>()), Times.Never);
            }
            #endregion

            #region Verify
            mockQueryProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryCallError()
        {
            #region Arrange
            var exception = new Exception(Helper.GenerateRandomString());

            var mockContractConnection = new Mock<IMappedContractConnection>();

            mockContractConnection.Setup(x => x.QueryAsync<BasicQuery, BasicQueryResponse>(It.IsAny<TransmissionMessage<BasicQuery>>(), It.IsAny<TimeSpan?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult<QueryResult<BasicQueryResponse>>(new(Helper.GenerateRandomString(10), new MessageHeader([]), Error: new(exception, false))));

            var cqrsConnection = mockContractConnection.Object.CreateCQRSConnection();

            var command = new BasicQuery("TestSimpleQuery");
            #endregion

            #region Act
            var error = await Assert.ThrowsAsync<QueryCallException>(async () => await cqrsConnection.ExecuteQueryAsync<BasicQuery, BasicQueryResponse>(command, cancellationToken: TestContext.CancellationToken));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            Assert.AreEqual(exception.Message, error.Error.Exception?.Message);
            Assert.IsFalse(error.Error.IsFatal);
            #endregion

            #region Verify
            mockContractConnection.Verify(x => x.QueryAsync<BasicQuery, BasicQueryResponse>(It.Is<TransmissionMessage<BasicQuery>>(msg=>Equals(msg.Message,command) && msg.Header!=null), null, null, null, It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestQueryWithTelemetry()
        {
            #region Arrange
            (var listener, _, var sourceName) = Helper.SetupTelemetry();
            var isSecondCallHeader = "isSecondCall";
            var receivedContexts = new List<IQueryInvocationContext<BasicQuery>>();

            var command = new BasicQuery(Helper.GenerateRandomString());
            var secondQuery = new BasicQuery(Helper.GenerateRandomString());

            var mockQueryProcessor = new Mock<IQueryProcessor<BasicQuery, BasicQueryResponse>>();

            mockQueryProcessor.Setup(x => x.ProcessQueryAsync(It.IsAny<IQueryInvocationContext<BasicQuery>>(), It.IsAny<CancellationToken>()))
                .Returns(async (IQueryInvocationContext<BasicQuery> context, CancellationToken cancellationToken) =>
                {
                    receivedContexts.Add(context);
                    if (string.IsNullOrEmpty(context[isSecondCallHeader]))
                    {
                        context[isSecondCallHeader] = "true";
                        return (await context.ExecuteQueryAsync<BasicQuery, BasicQueryResponse>(secondQuery))!;
                    }
                    return new BasicQueryResponse(context.Query.Name);
                });
            mockQueryProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(true);
            ((IMappedContractConnection)contractConnection).EnableOpenTelemetry(activitySource: sourceName);
            var cqrsConnection = await contractConnection.CreateCQRSConnection()
                .RegisterQueryProcessorAsync<BasicQuery, BasicQueryResponse>(mockQueryProcessor.Object, group: groupName)
                .RegisterQueryProcessorAsync<BasicQuery, BasicQueryResponse>(mockQueryProcessor.Object, group: groupName);


            var context = new Context();
            #endregion

            #region Act
            var result = await cqrsConnection.ExecuteQueryAsync<BasicQuery, BasicQueryResponse>(command, context: context, cancellationToken: TestContext.CancellationToken);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(receivedContexts, 2, TimeSpan.FromMinutes(1)));
            Assert.HasCount(2, receivedContexts);
            Assert.AreEqual(receivedContexts[0].CorrelationId, receivedContexts[0].Activity?.GetTagItem(Constants.CorrelationIdTag));
            Assert.AreEqual(receivedContexts[0].MessageId, receivedContexts[0].Activity?.GetTagItem(Constants.MessageIdTag));
            Assert.AreEqual(receivedContexts[0].CausationId, receivedContexts[0].Activity?.GetTagItem(Constants.CausationIdTag));
            Assert.AreEqual("query", receivedContexts[0].Activity?.GetTagItem(Constants.TypeTag));
            Assert.AreEqual(receivedContexts[1].CorrelationId, receivedContexts[1].Activity?.GetTagItem(Constants.CorrelationIdTag));
            Assert.AreEqual(receivedContexts[1].MessageId, receivedContexts[1].Activity?.GetTagItem(Constants.MessageIdTag));
            Assert.AreEqual(receivedContexts[1].CausationId, receivedContexts[1].Activity?.GetTagItem(Constants.CausationIdTag));
            Assert.AreEqual("query", receivedContexts[1].Activity?.GetTagItem(Constants.TypeTag));
            #endregion

            #region Verify
            mockQueryProcessor.Verify(x => x.ProcessQueryAsync(It.IsAny<IQueryInvocationContext<BasicQuery>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockQueryProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.Never);
            #endregion
        }

        public TestContext TestContext { get; set; }
    }
}
