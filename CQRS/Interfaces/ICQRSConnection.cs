using MQContract.CQRS.Interfaces.Command;
using MQContract.CQRS.Interfaces.Query;

namespace MQContract.CQRS.Interfaces
{
    /// <summary>
    /// The primary interface for all CQRS operations.  This is used to both execute commands/queries as well as to register processors for those items
    /// </summary>
    public interface ICQRSConnection : IAsyncDisposable
    {
        /// <summary>
        /// Called to execute a command of the given type
        /// </summary>
        /// <typeparam name="TCommand">The type of command to execute</typeparam>
        /// <param name="command">The command to execute</param>
        /// <param name="context">The context to use if desired</param>
        /// <param name="cancellationToken">A cancellation token to use if desired</param>
        /// <returns>A ValueTask to allow for async execution</returns>
        ValueTask ExecuteCommandAsync<TCommand>(TCommand command, Context? context = null, CancellationToken cancellationToken = default)
            where TCommand : ICommand;

        /// <summary>
        /// Called to execute a command of the given type that is expected to provide a given response
        /// </summary>
        /// <typeparam name="TCommand">The type of command to execute</typeparam>
        /// <typeparam name="TCommandResult">The type of response to expect</typeparam>
        /// <param name="command">The command to execute</param>
        /// <param name="context">The context to use if desired</param>
        /// <param name="timeout">The timeout to allow for the execution, if not specified the underlying contract connection defaults will apply</param>
        /// <param name="cancellationToken">A cancellation token to use if desired</param>
        /// <returns>The exepected result type</returns>
        ValueTask<TCommandResult?> ExecuteCommandAsync<TCommand, TCommandResult>(TCommand command, Context? context = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
            where TCommand : ICommand<TCommandResult>;

        /// <summary>
        /// Called to execute a query of the given type
        /// </summary>
        /// <typeparam name="TQuery">The type of query to execute</typeparam>
        /// <typeparam name="TQueryResponse">The type of response to expect</typeparam>
        /// <param name="query">The query to execute</param>
        /// <param name="context">The context to use if desired</param>
        /// <param name="timeout">The timeout to allow for the execution, if not specified the underlying contract connection defaults will apply</param>
        /// <param name="cancellationToken">A cancellation token to use if desired</param>
        /// <returns>The exepected result type</returns>
        ValueTask<TQueryResponse?> ExecuteQueryAsync<TQuery, TQueryResponse>(TQuery query, Context? context = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
            where TQuery : IQuery;

        /// <summary>
        /// Register a Command processor
        /// </summary>
        /// <typeparam name="TCommand">The type of command the processor handles</typeparam>
        /// <param name="processor">The command processor to register</param>
        /// <param name="group">The group name to assign it if desired</param>
        /// <returns>The underlying CQRS connection</returns>
        ValueTask<ICQRSConnection> RegisterCommandProcessorAsync<TCommand>(ICommandProcessor<TCommand> processor, string? group = null)
            where TCommand : ICommand;

        /// <summary>
        /// Register a Command processor for a command with a response
        /// </summary>
        /// <typeparam name="TCommand">The type of command the processor handles</typeparam>
        /// <typeparam name="TCommandResult">The type of response for the command call</typeparam>
        /// <param name="processor">The command processor to register</param>
        /// <param name="group">The group name to assign it if desired</param>
        /// <returns>The underlying CQRS connection</returns>
        ValueTask<ICQRSConnection> RegisterCommandProcessorAsync<TCommand, TCommandResult>(ICommandProcessor<TCommand, TCommandResult> processor, string? group = null)
            where TCommand : ICommand<TCommandResult>;

        /// <summary>
        /// Register a Query processor
        /// </summary>
        /// <typeparam name="TQuery">The type of Query the processor handles</typeparam>
        /// <typeparam name="TQueryResponse">The type of response for the query call</typeparam>
        /// <param name="processor">The query processor to register</param>
        /// <param name="group">The group name to assign it if desired</param>
        /// <returns>The underlying CQRS connection</returns>
        ValueTask<ICQRSConnection> RegisterQueryProcessorAsync<TQuery, TQueryResponse>(IQueryProcessor<TQuery, TQueryResponse> processor, string? group = null)
            where TQuery : IQuery;
    }
}
