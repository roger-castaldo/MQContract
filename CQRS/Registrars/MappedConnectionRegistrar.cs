using MQContract.CQRS.Consumers;
using MQContract.CQRS.Interfaces;
using MQContract.Interfaces;
using MQContract.Messages;

namespace MQContract.CQRS.Registrars
{
    internal class MappedConnectionRegistrar(IMappedContractConnection mappedConnection)
        : IProcessorRegistrar
    {
        async ValueTask IProcessorRegistrar.RegisterCommandProcessorAsync<TCommand>(CommandConsumer<TCommand> processor, string? group, MessageFilters<TCommand>? messageFilters)
            => await mappedConnection.RegisterPubSubAsyncConsumerAsync<TCommand, CommandConsumer<TCommand>>(
                    processor,
                    group: group,
                    messageFilters: messageFilters
                );

        async ValueTask IProcessorRegistrar.RegisterCommandProcessorAsync<TCommand, TCommandResult>(CommandResponseConsumer<TCommand, TCommandResult> processor, string? group, MessageFilters<TCommand>? messageFilters)
            => await mappedConnection.RegisterQueryResponseAsyncConsumerAsync<TCommand, TCommandResult, CommandResponseConsumer<TCommand, TCommandResult>>(
                processor,
                group: group,
                messageFilters: messageFilters
            );

        async ValueTask IProcessorRegistrar.RegisterQueryProcessorAsync<TQuery, TQueryResponse>(QueryResponseConsumer<TQuery, TQueryResponse> processor, string? group, MessageFilters<TQuery>? messageFilters)
            => await mappedConnection.RegisterQueryResponseAsyncConsumerAsync<TQuery, TQueryResponse, QueryResponseConsumer<TQuery, TQueryResponse>>(
                processor,
                group: group,
                messageFilters: messageFilters
            );
    }
}
