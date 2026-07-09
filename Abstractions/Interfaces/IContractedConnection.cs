namespace MQContract.Interfaces;

/// <summary>
/// The base representation of a Contract Connection, specifically a single service connection supporting contract connection
/// </summary>
public interface IContractedConnection : IContractConnection, IMetricContractConnection<IContractedConnection>
{
}
