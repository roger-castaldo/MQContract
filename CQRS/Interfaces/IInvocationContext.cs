using MQContract.CQRS.Interfaces.Command;
using MQContract.CQRS.Interfaces.Query;
using System.Diagnostics;

namespace MQContract.CQRS.Interfaces
{
    public interface IInvocationContext
    {
        Guid MessageId { get; }
        Guid CorrelationId { get; }
        Guid? CausationId { get; }
        string? this[string key] { get; set; }
        Activity? Activity { get; }

        ValueTask ExecuteCommandAsync<TCommand>(TCommand command)
            where TCommand : ICommand;
        ValueTask<TCommandResult?> ExecuteCommandAsync<TCommand, TCommandResult>(TCommand command)
            where TCommand : ICommand<TCommandResult>;

        ValueTask<TQueryResponse?> ExecuteQueryAsync<TQuery, TQueryResponse>(TQuery query)
            where TQuery : IQuery
            where TQueryResponse : IQueryResponse;
    }
}
