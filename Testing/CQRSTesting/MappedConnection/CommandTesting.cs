using CQRSTesting.Messages;
using Moq;
using MQContract;
using MQContract.CQRS;
using MQContract.CQRS.Extensions;
using MQContract.CQRS.Interfaces;
using MQContract.CQRS.Interfaces.Command;
using MQContract.Interfaces;
using MQContract.Messages;

namespace CQRSTesting.MappedConnection
{
    [TestClass]
    public class CommandTesting
    {
        [TestMethod]
        public async Task TestSimpleCommand()
        {
            #region Arrange
            var receivedCommands = new List<BasicCommand>();

            var mockCommandProcessor = new Mock<ICommandProcessor<BasicCommand>>();

            mockCommandProcessor.Setup(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicCommand>>(), It.IsAny<CancellationToken>()))
                .Returns((ICommandInvocationContext<BasicCommand>  context, CancellationToken cancellationToken) =>
                {
                    receivedCommands.Add(context.Command);
                    return ValueTask.CompletedTask;
                });
            mockCommandProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(true);
            var cqrsConnection = await contractConnection.CreateCQRSConnection()
                .RegisterCommandProcessorAsync<BasicCommand>(mockCommandProcessor.Object);

            var command = new BasicCommand("TestSimpleCommand");
            #endregion

            #region Act
            await cqrsConnection.ExecuteCommandAsync<BasicCommand>(command);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(receivedCommands, 1, TimeSpan.FromMinutes(1)));
            Assert.HasCount(1, receivedCommands);
            Assert.AreEqual(command, receivedCommands[0]);
            #endregion

            #region Verify
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockCommandProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()),Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestSimpleCommandWithContext()
        {
            #region Arrange
            var receivedContexts = new List<ICommandInvocationContext<BasicCommand>>();

            var mockCommandProcessor = new Mock<ICommandProcessor<BasicCommand>>();

            mockCommandProcessor.Setup(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicCommand>>(), It.IsAny<CancellationToken>()))
                .Returns((ICommandInvocationContext<BasicCommand> context, CancellationToken cancellationToken) =>
                {
                    receivedContexts.Add(context);
                    return ValueTask.CompletedTask;
                });
            mockCommandProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(true);
            var cqrsConnection = await contractConnection.CreateCQRSConnection()
                .RegisterCommandProcessorAsync<BasicCommand>(mockCommandProcessor.Object);

            var command = new BasicCommand("TestSimpleCommand");
            var context = new Context();
            context[Helper.GenerateRandomString()] = Helper.GenerateRandomString();
            #endregion

            #region Act
            await cqrsConnection.ExecuteCommandAsync<BasicCommand>(command,context:context);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(receivedContexts, 1, TimeSpan.FromMinutes(1)));
            Assert.HasCount(1, receivedContexts);
            Assert.AreEqual(command, receivedContexts[0].Command);
            Assert.HasCount(1, receivedContexts[0].Keys);
            Assert.IsNull(receivedContexts[0].CausationId);
            Assert.AreEqual(context.CorrelationId,receivedContexts[0].CorrelationId);
            Assert.AreEqual(context.MessageId,receivedContexts[0].MessageId);
            Assert.AreEqual(context[context.Keys.First()], receivedContexts[0][receivedContexts[0].Keys.First()]);
            #endregion

            #region Verify
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockCommandProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestCancellationTokenCallThroughCommand()
        {
            #region Arrange
            var receivedCommands = new List<BasicCommand>();
            var cancellationTokenSource = new CancellationTokenSource();

            var mockCommandProcessor = new Mock<ICommandProcessor<BasicCommand>>();

            mockCommandProcessor.Setup(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicCommand>>(), It.IsAny<CancellationToken>()))
                .Returns(async (ICommandInvocationContext<BasicCommand> context, CancellationToken cancellationToken) =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                    if (!cancellationToken.IsCancellationRequested)
                        receivedCommands.Add(context.Command);
                });
            mockCommandProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(true);
            var cqrsConnection = await contractConnection.CreateCQRSConnection("cancelCalls")
                .RegisterCommandProcessorAsync<BasicCommand>(mockCommandProcessor.Object);

            var command = new BasicCommand("TestSimpleCommand");
            #endregion

            #region Act
            await cqrsConnection.ExecuteCommandAsync<BasicCommand>(command,cancellationToken: cancellationTokenSource.Token);
            await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
            cancellationTokenSource.Cancel();
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            #endregion

            #region Assert
            Assert.HasCount(0, receivedCommands);
            #endregion

            #region Verify
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockCommandProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestCommandContextSharing()
        {
            #region Arrange
            var isSecondCallHeader = "isSecondCall";
            var receivedContexts = new List<ICommandInvocationContext<BasicCommand>>();

            var command = new BasicCommand(Helper.GenerateRandomString());
            var secondCommand = new BasicCommand(Helper.GenerateRandomString());

            var mockCommandProcessor = new Mock<ICommandProcessor<BasicCommand>>();

            mockCommandProcessor.Setup(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicCommand>>(), It.IsAny<CancellationToken>()))
                .Returns(async (ICommandInvocationContext<BasicCommand> context, CancellationToken cancellationToken) =>
                {
                    receivedContexts.Add(context);
                    if (string.IsNullOrEmpty(context[isSecondCallHeader]))
                    {
                        context[isSecondCallHeader] = "true";
                        await context.ExecuteCommandAsync(secondCommand);
                    }
                });
            mockCommandProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(true);
            var cqrsConnection = await contractConnection.CreateCQRSConnection()
                .RegisterCommandProcessorAsync<BasicCommand>(mockCommandProcessor.Object);

            
            var context = new Context();
            #endregion

            #region Act
            await cqrsConnection.ExecuteCommandAsync<BasicCommand>(command, context: context);
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
            #endregion

            #region Verify
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicCommand>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockCommandProcessor.Verify(x => x.ErrorRecieved(It.IsAny<Exception>()), Times.Never);
            #endregion
        }

        [TestMethod]
        public async Task TestCancellationTokenCallThroughChainedCommand()
        {
            #region Arrange
            var isSecondCallHeader = "isSecondCall";
            var receivedCommands = new List<BasicCommand>();
            var cancellationTokenSource = new CancellationTokenSource();

            var command = new BasicCommand(Helper.GenerateRandomString());
            var secondCommand = new BasicCommand(Helper.GenerateRandomString());

            var mockCommandProcessor = new Mock<ICommandProcessor<BasicCommand>>();

            mockCommandProcessor.Setup(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicCommand>>(), It.IsAny<CancellationToken>()))
                .Returns(async (ICommandInvocationContext<BasicCommand> context, CancellationToken cancellationToken) =>
                {
                    if (string.IsNullOrEmpty(context[isSecondCallHeader]))
                    {
                        receivedCommands.Add(context.Command);
                        context[isSecondCallHeader] = "true";
                        await context.ExecuteCommandAsync(secondCommand);
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                        if (!cancellationToken.IsCancellationRequested)
                            receivedCommands.Add(context.Command);
                    }
                });
            mockCommandProcessor.Setup(x => x.ErrorRecieved(It.IsAny<Exception>()));

            await using var contractConnection = Helper.ProduceConnection(true);
            var cqrsConnection = await contractConnection.CreateCQRSConnection("cancelCalls")
                .RegisterCommandProcessorAsync<BasicCommand>(mockCommandProcessor.Object);


            var context = new Context();
            #endregion

            #region Act
            await cqrsConnection.ExecuteCommandAsync<BasicCommand>(command, cancellationToken: cancellationTokenSource.Token);
            await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
            cancellationTokenSource.Cancel();
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            #endregion

            #region Assert
            Assert.IsTrue(await Helper.WaitForCount(receivedCommands, 1, TimeSpan.FromMinutes(1)));
            Assert.AreEqual(command, receivedCommands[0]);
            #endregion

            #region Verify
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicCommand>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
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
            var receivedCommands = new List<BasicCommand>();
            var dropped = false;

            var mockCommandProcessor = new Mock<ICommandProcessor<BasicCommand>>();

            mockCommandProcessor.Setup(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicCommand>>(), It.IsAny<CancellationToken>()))
                .Returns((ICommandInvocationContext<BasicCommand> context, CancellationToken cancellationToken) =>
                {
                    receivedCommands.Add(context.Command);
                    return ValueTask.CompletedTask;
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
                mockCommandProcessor.As<IFilteredCommandProcessor<BasicCommand>>().SetupGet(x=>x.Filter)
                    .Returns((BasicCommand command, Context context) => {
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
                .RegisterCommandProcessorAsync<BasicCommand>(mockCommandProcessor.Object);

            var command = new BasicCommand(messageValue);
            var context = new Context();
            context[headerKey] = headerValue;
            context[messageHeaderKey] = messageHeaderValue;
            #endregion

            #region Act
            await cqrsConnection.ExecuteCommandAsync<BasicCommand>(command, context:context);
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            #endregion

            #region Assert
            if (Equals(headerValue, checkValue) && Equals(messageHeaderValue, messageHeaderCheckValue) && Equals(messageValue, messageCheckValue))
            {
                Assert.IsTrue(await Helper.WaitForCount(receivedCommands, 1, TimeSpan.FromMinutes(1)));
                Assert.HasCount(1, receivedCommands);
                Assert.AreEqual(command, receivedCommands[0]);
                Assert.IsFalse(dropped);
                mockCommandProcessor.Verify(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
            }
            else
            {
                Assert.IsEmpty(receivedCommands);
                Assert.IsTrue(dropped);
                mockCommandProcessor.Verify(x => x.ProcessCommandAsync(It.IsAny<ICommandInvocationContext<BasicCommand>>(), It.IsAny<CancellationToken>()), Times.Never);
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

            var mockContractConnection = new Mock<IMappedContractConnection>();

            mockContractConnection.Setup(x => x.PublishAsync<BasicCommand>(It.IsAny<BasicCommand>(), It.IsAny<string>(), It.IsAny<MessageHeader>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult<TransmissionResult>(new(Helper.GenerateRandomString(10), Error: new(exception, false))));

            var cqrsConnection = mockContractConnection.Object.CreateCQRSConnection();

            var command = new BasicCommand("TestSimpleCommand");
            #endregion

            #region Act
            var error = await Assert.ThrowsAsync<CommandCallException>(async()=>await cqrsConnection.ExecuteCommandAsync<BasicCommand>(command));
            #endregion

            #region Assert
            Assert.IsNotNull(error);
            Assert.AreEqual(exception.Message, error.Error.Exception?.Message);
            Assert.IsFalse(error.Error.IsFatal);
            #endregion

            #region Verify
            mockContractConnection.Verify(x => x.PublishAsync<BasicCommand>(command, null, It.IsNotNull<MessageHeader>(), It.IsAny<CancellationToken>()), Times.Once);
            #endregion
        }
    }
}
