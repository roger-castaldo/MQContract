using MQContract.Messages;

namespace MQContract.Interfaces
{
    public interface IContractConnection
    {
        Task<IPingResult> PingAsync();
        Task<ITransmissionResult> PublishAsync<T>(T message, TimeSpan? timeout = null, string? channel = null, IMessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where T : class;
        Task<ISubscription> SubscribeAsync<T>(Action<IMessage<T>> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, bool ignoreMessageHeader = false,bool synchronous=false, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where T : class;
        Task<IQueryResult<R>> QueryAsync<Q, R>(Q message, TimeSpan? timeout = null, string? channel = null, IMessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class
            where R : class;
        Task<IQueryResult<object>> QueryAsync<Q>(Q message, TimeSpan? timeout = null, string? channel = null, IMessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class;
        Task<ISubscription> SubscribeQueryResponseAsync<Q,R>(Func<IMessage<Q>,Task<QueryResponseMessage<R>>> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, bool ignoreMessageHeader = false, bool synchronous = false, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class
            where R : class;
    }
}
