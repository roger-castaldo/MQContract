using MQContract.CQRS.Consumers;
using MQContract.CQRS.Interfaces.Command;
using MQContract.CQRS.Interfaces.Query;
using MQContract.Messages;

namespace MQContract.CQRS.Interfaces;

internal interface IProcessorRegistrar
{
    ValueTask RegisterCommandProcessorAsync<TCommand>(CommandConsumer<TCommand> processor, string? group = null, MessageFilters<TCommand>? messageFilters = null)
        where TCommand : ICommand;

    ValueTask RegisterCommandProcessorAsync<TCommand, TCommandResult>(CommandResponseConsumer<TCommand, TCommandResult> processor, string? group = null, MessageFilters<TCommand>? messageFilters = null)
        where TCommand : ICommand<TCommandResult>;
    ValueTask RegisterQueryProcessorAsync<TQuery, TQueryResponse>(QueryResponseConsumer<TQuery, TQueryResponse> processor, string? group = null, MessageFilters<TQuery>? messageFilters = null)
        where TQuery : IQuery;
}
