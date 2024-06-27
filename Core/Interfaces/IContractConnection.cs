using MQContract.ServiceAbstractions;
using MQContract.ServiceAbstractions.Messages;

namespace MQContract.Interfaces
{
    public interface IContractConnection
    {
        Task<IPingResult> PingAsync();
        Task<ITransmissionResult> PublishAsync<T>(T message, TimeSpan? timeout=null, string? channel = null, IMessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where T : class;
        Task<IQueryResult<R>> QueryAsync<Q, R>(Q message, TimeSpan? timeout=null, string? channel = null, IMessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class
            where R : class;
    }
}
