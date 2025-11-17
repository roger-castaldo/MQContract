using CQRSTesting.Messages;
using Moq;
using MQContract;
using MQContract.CQRS;
using MQContract.CQRS.Extensions;
using MQContract.CQRS.Interfaces;
using MQContract.CQRS.Interfaces.Command;
using MQContract.Interfaces;
using MQContract.Messages;

namespace CQRSTesting.ContractedConnection
{
    [TestClass]
    public class CommandWithResponseTesting
    {
        private const string groupName = "CommandWithResponseTesting";

        [TestMethod]
        public async Task TestSimpleCommand()
        {
            #region Arrange
            var receivedCommands = new List<BasicResponseCommand>();

            var mockCommandProcessor = new Mock<ICommandProcessor<BasicResponseCommand, BasicCommandResponse>>();

            mockCommandProcessor.Setup(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicResponseCommand>>(), It.IsAny<CancellationToken>()))
                .Returns((ICommandInvocationContext<BasicResponseCommand> context, CancellationToken cancellationToken) =>
                {
                    receivedCommands.Add(context.Command);
                    return ValueTask.FromResult<BasicCommandResponse>(new(context.Command.Name));
                });
            mockCommandProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(false);
            var cqrsConnection = await contractConnection.CreateCQRSConnection()
                .RegisterCommandProcessorAsync<BasicResponseCommand,BasicCommandResponse>(mockCommandProcessor.Object);

            var command = new BasicResponseCommand(Helper.GenerateRandomString());
            #endregion

            #region Act
            var result = await cqrsConnection.ExecuteCommandAsync<BasicResponseCommand,BasicCommandResponse>(command);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(receivedCommands, 1, TimeSpan.FromMinutes(1)));
            Assert.HasCount(1, receivedCommands);
            Assert.AreEqual(command, receivedCommands[0]);
            Assert.IsNotNull(result);
            Assert.AreEqual(command.Name, result.Name);
            #endregion

            #region Verify
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicResponseCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockCommandProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestSimpleCommandWithContext()
        {
            #region Arrange
            var receivedContexts = new List<ICommandInvocationContext<BasicResponseCommand>>();

            var mockCommandProcessor = new Mock<ICommandProcessor<BasicResponseCommand, BasicCommandResponse>>();

            mockCommandProcessor.Setup(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicResponseCommand>>(), It.IsAny<CancellationToken>()))
                .Returns((ICommandInvocationContext<BasicResponseCommand> context, CancellationToken cancellationToken) =>
                {
                    receivedContexts.Add(context);
                    return ValueTask.FromResult<BasicCommandResponse>(new(context.Command.Name));
                });
            mockCommandProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(false);
            var cqrsConnection = await contractConnection.CreateCQRSConnection()
                .RegisterCommandProcessorAsync<BasicResponseCommand, BasicCommandResponse>(mockCommandProcessor.Object, group: groupName);

            var command = new BasicResponseCommand(Helper.GenerateRandomString());
            var context = new Context();
            context[Helper.GenerateRandomString()] = Helper.GenerateRandomString();
            #endregion

            #region Act
            var result = await cqrsConnection.ExecuteCommandAsync<BasicResponseCommand, BasicCommandResponse>(command, context: context);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(receivedContexts, 1, TimeSpan.FromMinutes(1)));
            Assert.HasCount(1, receivedContexts);
            Assert.AreEqual(command, receivedContexts[0].Command);
            Assert.HasCount(1, receivedContexts[0].Keys);
            Assert.IsNull(receivedContexts[0].CausationId);
            Assert.AreEqual(context.CorrelationId, receivedContexts[0].CorrelationId);
            Assert.AreEqual(context.MessageId, receivedContexts[0].MessageId);
            Assert.AreEqual(context[context.Keys.First()], receivedContexts[0][receivedContexts[0].Keys.First()]);
            Assert.IsNotNull(result);
            Assert.AreEqual(command.Name, result.Name);
            #endregion

            #region Verify
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicResponseCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockCommandProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestCancellationTokenCallThroughCommand()
        {
            #region Arrange
            var receivedCommands = new List<BasicResponseCommand>();
            var cancellationTokenSource = new CancellationTokenSource();

            var mockCommandProcessor = new Mock<ICommandProcessor<BasicResponseCommand, BasicCommandResponse>>();

            mockCommandProcessor.Setup(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicResponseCommand>>(), It.IsAny<CancellationToken>()))
                .Returns(async (ICommandInvocationContext<BasicResponseCommand> context, CancellationToken cancellationToken) =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                    if (!cancellationToken.IsCancellationRequested)
                        receivedCommands.Add(context.Command);
                    return new(context.Command.Name);
                });
            mockCommandProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(false);
            var cqrsConnection = await contractConnection.CreateCQRSConnection("cancelCalls")
                .RegisterCommandProcessorAsync<BasicResponseCommand, BasicCommandResponse>(mockCommandProcessor.Object, group:groupName);

            var command = new BasicResponseCommand(Helper.GenerateRandomString());
            #endregion

            #region Act
            _ = Task.Delay(TimeSpan.FromSeconds(1))
                .ContinueWith((tsk) => cancellationTokenSource.Cancel());
            var err = await Assert.ThrowsAsync<CommandCallException>(async () => await cqrsConnection.ExecuteCommandAsync<BasicResponseCommand, BasicCommandResponse>(command, cancellationToken: cancellationTokenSource.Token));
            #endregion

            #region Assert
            Assert.HasCount(0, receivedCommands);
            Assert.IsNotNull(err);
            Assert.IsInstanceOfType<TaskCanceledException>(err.Error.Exception);
            #endregion

            #region Verify
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicResponseCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockCommandProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestCommandContextSharing()
        {
            #region Arrange
            var isSecondCallHeader = "isSecondCall";
            var receivedContexts = new List<ICommandInvocationContext<BasicResponseCommand>>();

            var command = new BasicResponseCommand(Helper.GenerateRandomString());
            var secondCommand = new BasicResponseCommand(Helper.GenerateRandomString());

            var mockCommandProcessor = new Mock<ICommandProcessor<BasicResponseCommand, BasicCommandResponse>>();

            mockCommandProcessor.Setup(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicResponseCommand>>(), It.IsAny<CancellationToken>()))
                .Returns(async (ICommandInvocationContext<BasicResponseCommand> context, CancellationToken cancellationToken) =>
                {
                    receivedContexts.Add(context);
                    if (string.IsNullOrEmpty(context[isSecondCallHeader]))
                    {
                        context[isSecondCallHeader] = "true";
                        return (await context.ExecuteCommandAsync<BasicResponseCommand, BasicCommandResponse>(secondCommand))!;
                    }
                    return new BasicCommandResponse(context.Command.Name);
                });
            mockCommandProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(false);
            var cqrsConnection = await contractConnection.CreateCQRSConnection()
                .RegisterCommandProcessorAsync<BasicResponseCommand, BasicCommandResponse>(mockCommandProcessor.Object, group: groupName)
                .RegisterCommandProcessorAsync<BasicResponseCommand, BasicCommandResponse>(mockCommandProcessor.Object, group: groupName);


            var context = new Context();
            #endregion

            #region Act
            var result = await cqrsConnection.ExecuteCommandAsync<BasicResponseCommand, BasicCommandResponse>(command, context: context);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(receivedContexts, 2, TimeSpan.FromMinutes(1)));
            Assert.HasCount(2, receivedContexts);
            Assert.AreEqual(command, receivedContexts[0].Command);
            Assert.HasCount(1, receivedContexts[0].Keys);
            Assert.IsNull(receivedContexts[0].CausationId);
            Assert.AreEqual(context.CorrelationId, receivedContexts[0].CorrelationId);
            Assert.AreEqual(context.MessageId, receivedContexts[0].MessageId);
            Assert.HasCount(1, receivedContexts[1].Keys);
            Assert.AreEqual(context.CorrelationId, receivedContexts[1].CorrelationId);
            Assert.AreEqual(context.MessageId, receivedContexts[1].CausationId);
            Assert.AreNotEqual(context.MessageId, receivedContexts[1].MessageId);
            Assert.AreEqual(secondCommand, receivedContexts[1].Command);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Name, secondCommand.Name);
            #endregion

            #region Verify
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicResponseCommand>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockCommandProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestCancellationTokenCallThroughChainedCommand()
        {
            #region Arrange
            var isSecondCallHeader = "isSecondCall";
            var receivedCommands = new List<BasicResponseCommand>();
            var cancellationTokenSource = new CancellationTokenSource();

            var command = new BasicResponseCommand(Helper.GenerateRandomString());
            var secondCommand = new BasicResponseCommand(Helper.GenerateRandomString());

            var mockCommandProcessor = new Mock<ICommandProcessor<BasicResponseCommand, BasicCommandResponse>>();

            mockCommandProcessor.Setup(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicResponseCommand>>(), It.IsAny<CancellationToken>()))
                .Returns(async (ICommandInvocationContext<BasicResponseCommand> context, CancellationToken cancellationToken) =>
                {
                    if (string.IsNullOrEmpty(context[isSecondCallHeader]))
                    {
                        receivedCommands.Add(context.Command);
                        context[isSecondCallHeader] = "true";
                        return (await context.ExecuteCommandAsync<BasicResponseCommand, BasicCommandResponse>(secondCommand))!;
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                        if (!cancellationToken.IsCancellationRequested)
                            receivedCommands.Add(context.Command);
                        return new BasicCommandResponse(context.Command.Name);
                    }
                });
            mockCommandProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(false);
            var cqrsConnection = await contractConnection.CreateCQRSConnection("cancelCalls")
                .RegisterCommandProcessorAsync<BasicResponseCommand, BasicCommandResponse>(mockCommandProcessor.Object, group: groupName)
                .RegisterCommandProcessorAsync<BasicResponseCommand, BasicCommandResponse>(mockCommandProcessor.Object, group: groupName);


            var context = new Context();
            #endregion

            #region Act
            _ = Task.Delay(TimeSpan.FromSeconds(1))
                .ContinueWith((tsk) => cancellationTokenSource.Cancel());
            var err = await Assert.ThrowsAsync<CommandCallException>(async()=>await cqrsConnection.ExecuteCommandAsync<BasicResponseCommand, BasicCommandResponse>(command, cancellationToken: cancellationTokenSource.Token));
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(receivedCommands, 1, TimeSpan.FromMinutes(1)));
            Assert.AreEqual(command, receivedCommands[0]);
            Assert.IsNotNull(err);
            Assert.IsInstanceOfType<TaskCanceledException>(err.Error.Exception);
            #endregion

            #region Verify
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicResponseCommand>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockCommandProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.Never);
            #endregion
        }

        [TestMethod]
        [DataRow("headerValue", "differentHeaderValue", "messageHeaderValue", "messageHeaderValue", "messageValue", "messageValue")]
        [DataRow("headerValue", "headerValue", "messageHeaderValue", "differentMessageHeaderValue", "messageValue", "messageValue")]
        [DataRow("headerValue", "headerValue", "messageHeaderValue", "messageHeaderValue", "messageValue", "differentMessageValue")]
        [DataRow("headerValue", "headerValue", "messageHeaderValue", "messageHeaderValue", "messageValue", "messageValue")]
        public async Task TestCommandProcessorWithFiltering(string headerValue, string checkValue, string messageHeaderValue, string messageHeaderCheckValue,
            string messageValue, string messageCheckValue)
        {
            #region Arrange
            var headerKey = "testHeader";
            var messageHeaderKey = "testMessageHeader";
            var receivedCommands = new List<BasicResponseCommand>();
            var dropped = false;

            var mockCommandProcessor = new Mock<ICommandProcessor<BasicResponseCommand, BasicCommandResponse>>();

            mockCommandProcessor.Setup(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicResponseCommand>>(), It.IsAny<CancellationToken>()))
                .Returns((ICommandInvocationContext<BasicResponseCommand> context, CancellationToken cancellationToken) =>
                {
                    receivedCommands.Add(context.Command);
                    return ValueTask.FromResult<BasicCommandResponse>(new(context.Command.Name));
                });
            mockCommandProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            if (!Equals(headerValue, checkValue))
                mockCommandProcessor.As<IContextFilteredProcessor>().SetupGet(x => x.Filter)
                    .Returns((Context context) => {
                        if (!Equals(context[headerKey], checkValue))
                        {
                            dropped=true;
                            return ValueTask.FromResult<MessageFilterResult>(MessageFilterResult.DropAndAcknowledge);
                        }
                        return ValueTask.FromResult<MessageFilterResult>(MessageFilterResult.Allow);
                    });
            if (Equals(headerValue, checkValue))
                mockCommandProcessor.As<IFilteredCommandProcessor<BasicResponseCommand>>().SetupGet(x => x.Filter)
                    .Returns((BasicResponseCommand command, Context context) => {
                        (var isDropped, var result) = (Equals(context[messageHeaderKey], messageHeaderCheckValue), Equals(command.Name, messageCheckValue)) switch
                        {
                            (true, true) => (false, MessageFilterResult.Allow),
                            (false, _) => (true, MessageFilterResult.DropAndAcknowledge),
                            (_, false) => (true, MessageFilterResult.DropAndDontAcknowledge)
                        };
                        dropped = isDropped;
                        return ValueTask.FromResult(result);
                    });

            await using var contractConnection = Helper.ProduceConnection(false);
            var cqrsConnection = await contractConnection.CreateCQRSConnection()
                .RegisterCommandProcessorAsync<BasicResponseCommand, BasicCommandResponse>(mockCommandProcessor.Object);

            var command = new BasicResponseCommand(messageValue);
            var context = new Context();
            context[headerKey] = headerValue;
            context[messageHeaderKey] = messageHeaderValue;

            BasicCommandResponse? result = null;
            Exception? error = null;
            #endregion

            #region Act
            if (Equals(headerValue, checkValue) && Equals(messageHeaderValue, messageHeaderCheckValue) && Equals(messageValue, messageCheckValue))
                result = await cqrsConnection.ExecuteCommandAsync<BasicResponseCommand, BasicCommandResponse>(command, context: context);
            else
                error = await Assert.ThrowsAsync<CommandTimeoutException>(async () => await cqrsConnection.ExecuteCommandAsync<BasicResponseCommand, BasicCommandResponse>(command, context: context, timeout: TimeSpan.FromMilliseconds(500)));
            #endregion

            #region Assert
            if (Equals(headerValue, checkValue) && Equals(messageHeaderValue, messageHeaderCheckValue) && Equals(messageValue, messageCheckValue))
            {
                Assert.IsTrue(await Helper.WaitForCount(receivedCommands, 1, TimeSpan.FromMinutes(1)));
                Assert.HasCount(1, receivedCommands);
                Assert.AreEqual(command, receivedCommands[0]);
                Assert.IsFalse(dropped);
                Assert.IsNull(error);
                Assert.IsNotNull(result);
                Assert.AreEqual(command.Name, result.Name);
                mockCommandProcessor.Verify(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicResponseCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
            }
            else
            {
                Assert.IsEmpty(receivedCommands);
                Assert.IsTrue(dropped);
                Assert.IsNotNull(error);
                Assert.IsInstanceOfType<CommandTimeoutException>(error);
                Assert.IsNull(result);
                mockCommandProcessor.Verify(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicResponseCommand>>(), It.IsAny<CancellationToken>()), Times.Never);
            }
            #endregion

            #region Verify
            mockCommandProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestCommandCallError()
        {
            #region Arrange
            var exception = new Exception(Helper.GenerateRandomString());

            var mockContractConnection = new Mock<IContractedConnection>();

            mockContractConnection.Setup(x => x.QueryAsync<BasicResponseCommand,BasicCommandResponse>(It.IsAny<BasicResponseCommand>(),It.IsAny<TimeSpan?>(), It.IsAny<string?>(),It.IsAny<string?>(), 
                It.IsAny<MessageHeader>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult<QueryResult<BasicCommandResponse>>(new(Helper.GenerateRandomString(10),new MessageHeader([]), Error: new(exception, false))));

            var cqrsConnection = mockContractConnection.Object.CreateCQRSConnection();

            var command = new BasicResponseCommand("TestSimpleCommand");
            #endregion

            #region Act
            var error = await Assert.ThrowsAsync<CommandCallException>(async () => await cqrsConnection.ExecuteCommandAsync<BasicResponseCommand,BasicCommandResponse>(command));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            Assert.AreEqual(exception.Message, error.Error.Exception?.Message);
            Assert.IsFalse(error.Error.IsFatal);
            #endregion

            #region Verify
            mockContractConnection.Verify(x => x.QueryAsync<BasicResponseCommand, BasicCommandResponse>(command, null,null,null, It.IsNotNull<MessageHeader>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }

        [TestMethod]
        public async Task TestCommandWithTelemetry()
        {
            #region Arrange
            (var listener, _, var sourceName) = Helper.SetupTelemetry();
            var isSecondCallHeader = "isSecondCall";
            var receivedContexts = new List<ICommandInvocationContext<BasicResponseCommand>>();

            var command = new BasicResponseCommand(Helper.GenerateRandomString());
            var secondCommand = new BasicResponseCommand(Helper.GenerateRandomString());

            var mockCommandProcessor = new Mock<ICommandProcessor<BasicResponseCommand, BasicCommandResponse>>();

            mockCommandProcessor.Setup(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicResponseCommand>>(), It.IsAny<CancellationToken>()))
                .Returns(async (ICommandInvocationContext<BasicResponseCommand> context, CancellationToken cancellationToken) =>
                {
                    receivedContexts.Add(context);
                    if (string.IsNullOrEmpty(context[isSecondCallHeader]))
                    {
                        context[isSecondCallHeader] = "true";
                        return (await context.ExecuteCommandAsync<BasicResponseCommand, BasicCommandResponse>(secondCommand))!;
                    }
                    return new BasicCommandResponse(context.Command.Name);
                });
            mockCommandProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(false);
            ((IContractedConnection)contractConnection).EnableOpenTelemetry(activitySource: sourceName);
            var cqrsConnection = await contractConnection.CreateCQRSConnection()
                .RegisterCommandProcessorAsync<BasicResponseCommand, BasicCommandResponse>(mockCommandProcessor.Object, group: groupName)
                .RegisterCommandProcessorAsync<BasicResponseCommand, BasicCommandResponse>(mockCommandProcessor.Object, group: groupName);


            var context = new Context();
            #endregion

            #region Act
            var result = await cqrsConnection.ExecuteCommandAsync<BasicResponseCommand, BasicCommandResponse>(command, context: context);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(receivedContexts, 2, TimeSpan.FromMinutes(1)));
            Assert.HasCount(2, receivedContexts);
            Assert.AreEqual(receivedContexts[0].CorrelationId, receivedContexts[0].Activity?.GetTagItem(Constants.CorrelationIdTag));
            Assert.AreEqual(receivedContexts[0].MessageId, receivedContexts[0].Activity?.GetTagItem(Constants.MessageIdTag));
            Assert.AreEqual(receivedContexts[0].CausationId, receivedContexts[0].Activity?.GetTagItem(Constants.CausationIdTag));
            Assert.AreEqual("command", receivedContexts[0].Activity?.GetTagItem(Constants.TypeTag));
            Assert.AreEqual(receivedContexts[1].CorrelationId, receivedContexts[1].Activity?.GetTagItem(Constants.CorrelationIdTag));
            Assert.AreEqual(receivedContexts[1].MessageId, receivedContexts[1].Activity?.GetTagItem(Constants.MessageIdTag));
            Assert.AreEqual(receivedContexts[1].CausationId, receivedContexts[1].Activity?.GetTagItem(Constants.CausationIdTag));
            Assert.AreEqual("command", receivedContexts[1].Activity?.GetTagItem(Constants.TypeTag));
            #endregion

            #region Verify
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicResponseCommand>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockCommandProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.Never);
            #endregion
        }
    }
}
