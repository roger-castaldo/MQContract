using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Interfaces
{
    public interface IContractConnection
    {
        Task<PingResult> PingAsync();
        Task<TransmissionResult> PublishAsync<T>(T message, TimeSpan? timeout = null, string? channel = null, IMessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where T : class;
        Task<ISubscription> SubscribeAsync<T>(Func<IMessage<T>,Task> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, bool ignoreMessageHeader = false,bool synchronous=false, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where T : class;
        Task<QueryResult<R>> QueryAsync<Q, R>(Q message, TimeSpan? timeout = null, string? channel = null, IMessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class
            where R : class;
        Task<QueryResult<object>> QueryAsync<Q>(Q message, TimeSpan? timeout = null, string? channel = null, IMessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class;
        Task<ISubscription> SubscribeQueryResponseAsync<Q,R>(Func<IMessage<Q>,Task<QueryResponseMessage<R>>> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, bool ignoreMessageHeader = false, bool synchronous = false, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class
            where R : class;
    }
}
