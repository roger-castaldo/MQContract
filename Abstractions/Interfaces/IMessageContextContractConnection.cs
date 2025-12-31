namespace MQContract.Interfaces
{
    public interface IMessageContextContractConnection<TContractConnection> : IBaseContractConnection
        where TContractConnection : IBaseContractConnection
    {
        TContractConnection RegisterMessageContext(MQContractMessageContext messageContext);
    }
}
