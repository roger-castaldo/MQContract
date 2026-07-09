using CoreTesting.Messages;
using Moq;
using MQContract;
using MQContract.Interfaces.Service;

namespace CoreTesting.ConnectionTests;

[TestClass]
public class ResillianceSetupTests
{
    [TestMethod]
    public void TestRegisterDefaultPolicyWithInvalidValues()
    {
        #region Arrange
        var serviceConnection = new Mock<IMessageServiceConnection>();

        var contractConnection = ContractConnection.Instance(serviceConnection.Object);
        #endregion

        #region Act
        Assert.Throws<InvalidPolicyArgumentsException>(() => contractConnection.RegisterResiliencePolicy(null, null));
        Assert.Throws<InvalidRetryCircuitBreakTriggersException>(() => contractConnection.RegisterResiliencePolicy((2, (cnt) => TimeSpan.FromMilliseconds(100)), (1, TimeSpan.FromMilliseconds(100))));
        #endregion
    }

    [TestMethod]
    public void TestRegisterGenericPolicyWithInvalidValues()
    {
        #region Arrange
        var serviceConnection = new Mock<IMessageServiceConnection>();

        var contractConnection = ContractConnection.Instance(serviceConnection.Object);
        #endregion

        #region Act
        Assert.Throws<InvalidPolicyArgumentsException>(() => contractConnection.RegisterResiliencePolicy<BasicMessage>(null, null));
        Assert.Throws<InvalidRetryCircuitBreakTriggersException>(() => contractConnection.RegisterResiliencePolicy<BasicMessage>((2, (cnt) => TimeSpan.FromMilliseconds(100)), (1, TimeSpan.FromMilliseconds(100))));
        #endregion
    }

    [TestMethod]
    public void TestRegisterMessageTypePolicyWithInvalidValues()
    {
        #region Arrange
        var serviceConnection = new Mock<IMessageServiceConnection>();

        var contractConnection = ContractConnection.Instance(serviceConnection.Object);
        #endregion

        #region Act
        Assert.Throws<InvalidPolicyArgumentsException>(() => contractConnection.RegisterResiliencePolicy(typeof(BasicMessage), null, null));
        Assert.Throws<InvalidRetryCircuitBreakTriggersException>(() => contractConnection.RegisterResiliencePolicy(typeof(BasicMessage), (2, (cnt) => TimeSpan.FromMilliseconds(100)), (1, TimeSpan.FromMilliseconds(100))));
        #endregion
    }

    [TestMethod]
    public void TestRegisterChannelPolicyWithInvalidValues()
    {
        #region Arrange
        var serviceConnection = new Mock<IMessageServiceConnection>();

        var contractConnection = ContractConnection.Instance(serviceConnection.Object);
        #endregion

        #region Act
        Assert.Throws<InvalidPolicyArgumentsException>(() => contractConnection.RegisterResiliencePolicy("testChannel", null, null));
        Assert.Throws<InvalidRetryCircuitBreakTriggersException>(() => contractConnection.RegisterResiliencePolicy("testChannel", (2, (cnt) => TimeSpan.FromMilliseconds(100)), (1, TimeSpan.FromMilliseconds(100))));
        #endregion
    }
}
