using MQContract.CQRS.Interfaces;
using MQContract.CQRS.Interfaces.Command;
using MQContract.CQRS.Interfaces.Query;

namespace MQContract.CQRS.Extensions;

/// <summary>
/// Extension calls used to provide a Fluent style set of calls for registering the different processors.
/// </summary>
public static class ICQRSConnectionExtension
{
    /// <summary>
    /// Register a Command processor
    /// </summary>
    /// <typeparam name="TCommand">The type of command the processor handles</typeparam>
    /// <param name="cqrsConnectionTask">The previous registration call task</param>
    /// <param name="processor">The command processor to register</param>
    /// <param name="group">The group name to assign it if desired</param>
    /// <returns>The underlying CQRS connection</returns>
    public static async ValueTask<ICQRSConnection> RegisterCommandProcessorAsync<TCommand>(this ValueTask<ICQRSConnection> cqrsConnectionTask, ICommandProcessor<TCommand> processor, string? group = null)
        where TCommand : ICommand
    {
        var result = await cqrsConnectionTask.ConfigureAwait(false);
        return await result.RegisterCommandProcessorAsync<TCommand>(processor, group)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Register a Command processor for a command with a response
    /// </summary>
    /// <typeparam name="TCommand">The type of command the processor handles</typeparam>
    /// <typeparam name="TCommandResult">The type of response for the command call</typeparam>
    /// <param name="cqrsConnectionTask">The previous registration call task</param>
    /// <param name="processor">The command processor to register</param>
    /// <param name="group">The group name to assign it if desired</param>
    /// <returns>The underlying CQRS connection</returns>
    public static async ValueTask<ICQRSConnection> RegisterCommandProcessorAsync<TCommand, TCommandResult>(this ValueTask<ICQRSConnection> cqrsConnectionTask, ICommandProcessor<TCommand, TCommandResult> processor, string? group = null)
        where TCommand : ICommand<TCommandResult>
    {
        var result = await cqrsConnectionTask.ConfigureAwait(false);
        return await result.RegisterCommandProcessorAsync<TCommand, TCommandResult>(processor, group)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Register a Query processor
    /// </summary>
    /// <typeparam name="TQuery">The type of Query the processor handles</typeparam>
    /// <typeparam name="TQueryResponse">The type of response for the query call</typeparam>
    /// <param name="cqrsConnectionTask">The previous registration call task</param>
    /// <param name="processor">The query processor to register</param>
    /// <param name="group">The group name to assign it if desired</param>
    /// <returns>The underlying CQRS connection</returns>
    public static async ValueTask<ICQRSConnection> RegisterQueryProcessorAsync<TQuery, TQueryResponse>(this ValueTask<ICQRSConnection> cqrsConnectionTask, IQueryProcessor<TQuery, TQueryResponse> processor, string? group = null)
        where TQuery : IQuery
    {
        var result = await cqrsConnectionTask.ConfigureAwait(false);
        return await result.RegisterQueryProcessorAsync<TQuery, TQueryResponse>(processor, group)
            .ConfigureAwait(false);
    }
}
