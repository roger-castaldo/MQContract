namespace MQContract.Interfaces;

/// <summary>
/// Houses the message context pieces for a given contract connection
/// </summary>
/// <typeparam name="TContractConnection">The underlying type that is being represented here which must be IBaseContractConnection, CC is used for method chaining.</typeparam>
public interface IMessageContextContractConnection<TContractConnection> : IBaseContractConnection
    where TContractConnection : IBaseContractConnection
{
    /// <summary>
    /// Called to register a Message Context with the given connection
    /// </summary>
    /// <param name="messageContext">The Message Context (that the code generator has built upon) to register</param>
    /// <returns>The Contract Connection instance</returns>
    ValueTask<TContractConnection> RegisterMessageContextAsync(MQContractMessageContext messageContext);
}
