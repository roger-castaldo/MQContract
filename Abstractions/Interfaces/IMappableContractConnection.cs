using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Interfaces
{
    public interface IMappableContractConnection<CC> : IBaseContractConnection
        where CC : IBaseContractConnection
    {
        CC RegisterServiceConnection(Func<(string channel, Type messageType, MessageHeader messageHeader), bool> checkCallback, string serviceConnectionName, IMessageServiceConnection messageServiceConnection);
        CC RegisterServiceConnection(string channel, string serviceConnectionName, IMessageServiceConnection messageServiceConnection);
        CC RegisterServiceConnection(Type messageType, string serviceConnectionName, IMessageServiceConnection messageServiceConnection);
        CC RegisterServiceConnection(string messageHeaderKey, string messageHeaderValue, string serviceConnectionName, IMessageServiceConnection messageServiceConnection);
    }
}
