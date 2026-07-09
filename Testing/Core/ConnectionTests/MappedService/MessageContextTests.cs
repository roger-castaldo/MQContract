using CoreTesting.Messages;
using Moq;
using MQContract;
using MQContract.Extensions;
using MQContract.Interfaces;
using MQContract.Interfaces.Middleware;
using MQContract.Interfaces.Service;

namespace CoreTesting.ConnectionTests.MappedService;

[TestClass]
public class MessageContextTests
{
    private const string ServiceName = "testService";

    [TestMethod]
    public async Task TestRegisterExtension()
    {
        #region Arrange
        var mockConnection = new Mock<IMappedContractConnection>();
        mockConnection.Setup(x=>x.RegisterMessageContextAsync(It.IsAny<MQContractMessageContext>()))
            .Returns(ValueTask.FromResult(mockConnection.Object));

        var mockContext = new Mock<MQContractMessageContext>();

        var connectionTask = new ValueTask<IMappedContractConnection>(mockConnection.Object);
        #endregion

        #region Act
        var result = await connectionTask.RegisterMessageContextAsync(mockContext.Object);
        #endregion

        #region Assert
        Assert.AreEqual(mockConnection.Object, result);
        #endregion

        #region Verify
        mockConnection.Verify(x => x.RegisterMessageContextAsync(mockContext.Object), Times.Once);
        #endregion
    }

    [TestMethod]
    public async Task TestRegisterContextInvokesMiddleware()
    {
        #region Arrange
        var mockConnection = new Mock<IMessageServiceConnection>();
        var mockMiddleware = new Mock<IMessageContextAwareMiddleware>();

        mockMiddleware.Setup(x => x.ProcessMessagesFromMessageContextAsync(It.IsAny<IEnumerable<MessageContextDefintion>>()))
            .Returns(ValueTask.CompletedTask);

        IEnumerable<MessageContextDefintion> definitions = [
            new MessageContextDefintion(typeof(BasicMessage),"Broadcast.BasicMessage","BasicMessage-0.0.0.0",null,null)
        ];

        var mockContext = new Mock<MQContractMessageContext>();

        mockContext.Setup(x => x.DefinedMessages)
            .Returns(definitions);

        var connection = await ContractConnection.MappedServiceInstance()
            .RegisterServiceConnection((props) => true, ServiceName, mockConnection.Object)
            .RegisterMiddlewareAsync(mockMiddleware.Object);
        #endregion

        #region Act
        await connection.RegisterMessageContextAsync(mockContext.Object);
        #endregion

        #region Assert
        #endregion

        #region Verify
        mockMiddleware.Verify(x => x.ProcessMessagesFromMessageContextAsync(It.Is<IEnumerable<MessageContextDefintion>>(
            x=>x.SequenceEqual(definitions)
        )), Times.Once);
        mockContext.Verify(x => x.DefinedMessages, Times.Once);
        #endregion
    }

    [TestMethod]
    public async Task TestRegisterMiddlewareInvokesContext()
    {
        #region Arrange
        var mockConnection = new Mock<IMessageServiceConnection>();
        var mockMiddleware = new Mock<IMessageContextAwareMiddleware>();

        mockMiddleware.Setup(x => x.ProcessMessagesFromMessageContextAsync(It.IsAny<IEnumerable<MessageContextDefintion>>()))
            .Returns(ValueTask.CompletedTask);

        IEnumerable<MessageContextDefintion> definitions = [
            new MessageContextDefintion(typeof(BasicMessage),"Broadcast.BasicMessage","BasicMessage-0.0.0.0",null,null)
        ];

        var mockContext = new Mock<MQContractMessageContext>();

        mockContext.Setup(x => x.DefinedMessages)
            .Returns(definitions);

        var connection = await ContractConnection.MappedServiceInstance()
            .RegisterServiceConnection((props) => true, ServiceName, mockConnection.Object)
            .RegisterMessageContextAsync(mockContext.Object);
        #endregion

        #region Act
        await connection.RegisterMiddlewareAsync(mockMiddleware.Object);
        #endregion

        #region Assert
        #endregion

        #region Verify
        mockMiddleware.Verify(x => x.ProcessMessagesFromMessageContextAsync(It.Is<IEnumerable<MessageContextDefintion>>(
            x => x.SequenceEqual(definitions)
        )), Times.Once);
        mockContext.Verify(x => x.DefinedMessages, Times.Once);
        #endregion
    }

}
