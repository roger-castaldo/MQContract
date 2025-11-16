using MQContract.CQRS.Interfaces.Command;
using MQContract.CQRS.Interfaces.Query;

namespace MQContract.CQRS.Interfaces
{
    public interface ICQRSConnection
    {
        ValueTask ExecuteCommandAsync<TCommand>(TCommand command, Context? context = null, CancellationToken cancellationToken = default)
            where TCommand : ICommand;
        ValueTask<TCommandResult?> ExecuteCommandAsync<TCommand, TCommandResult>(TCommand command, Context? context = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
            where TCommand : ICommand<TCommandResult>;
        ValueTask<TQueryResponse?> ExecuteQueryAsync<TQuery, TQueryResponse>(TQuery query, Context? context = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
            where TQuery : IQuery;
        ValueTask<ICQRSConnection> RegisterCommandProcessorAsync<TCommand>(ICommandProcessor<TCommand> processor, string? group = null)
            where TCommand : ICommand;

        ValueTask<ICQRSConnection> RegisterCommandProcessorAsync<TCommand, TCommandResult>(ICommandProcessor<TCommand, TCommandResult> processor, string? group = null)
            where TCommand : ICommand<TCommandResult>;
        ValueTask<ICQRSConnection> RegisterQueryProcessorAsync<TQuery, TQueryResponse>(IQueryProcessor<TQuery, TQueryResponse> processor, string? group = null)
            where TQuery : IQuery;
    }
}
