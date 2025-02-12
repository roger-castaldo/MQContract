using MQContract.Interfaces.Consumers;

namespace MQContract.Interfaces
{
    public interface IConsumerContractConnection : IBaseContractConnection
    {
        ValueTask<bool> RegisterPubSubConsumerAsync<T, TConsumer>(TConsumer consumer, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken())
            where TConsumer : IPubSubConsumer<T>;
        ValueTask<bool> RegisterPubSubConsumerAsync<T, TConsumer>(string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken())
            where TConsumer : IPubSubConsumer<T>;
        ValueTask<bool> RegisterPubSubConsumerAsync(Type consumerType, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken());

        ValueTask<bool> RegisterPubSubAsyncConsumerAsync<T, TConsumer>(TConsumer consumer, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken())
            where TConsumer : IPubSubAsyncConsumer<T>;
        ValueTask<bool> RegisterPubSubAsyncConsumerAsync<T, TConsumer>(string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken())
            where TConsumer : IPubSubAsyncConsumer<T>;
        ValueTask<bool> RegisterPubSubAsyncConsumerAsync(Type consumerType, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken());

        ValueTask<bool> RegisterQueryResponseConsumerAsync<Q, R, TConsumer>(TConsumer consumer, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken())
            where TConsumer : IQueryResponseConsumer<Q,R>;

        ValueTask<bool> RegisterQueryResponseConsumerAsync<Q, R, TConsumer>(string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken())
            where TConsumer : IQueryResponseConsumer<Q, R>;

        ValueTask<bool> RegisterQueryResponseConsumerAsync(Type consumerType, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken());

        ValueTask<bool> RegisterQueryResponseAsyncConsumerAsync<Q, R, TConsumer>(TConsumer consumer, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken())
            where TConsumer : IQueryResponseAsyncConsumer<Q, R>;

        ValueTask<bool> RegisterQueryResponseAsyncConsumerAsync<Q, R, TConsumer>(string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken())
            where TConsumer : IQueryResponseAsyncConsumer<Q, R>;

        ValueTask<bool> RegisterQueryResponseAsyncConsumerAsync(Type consumerType, string? channel = null, string? group = null, bool ignoreMessageHeader = false, CancellationToken cancellationToken = new CancellationToken());
    }
}
