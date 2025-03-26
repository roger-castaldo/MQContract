namespace MQContract.Interfaces
{
    /// <summary>
    /// The representation of a Mapped Contract Connection which is built to use 1 or more service connections for the calls
    /// </summary>
    public interface IMappedContractConnection
        : IContractConnection, IMetricContractConnection<IMappedContractConnection>, IMappableContractConnection<IMappedContractConnection>
    {
    }
}
