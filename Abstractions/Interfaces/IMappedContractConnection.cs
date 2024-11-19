namespace MQContract.Interfaces
{
    public interface IMappedContractConnection 
        : IContractConnection, IMetricContractConnection<IMappedContractConnection>, IMappableContractConnection<IMappedContractConnection>
    {
    }
}
