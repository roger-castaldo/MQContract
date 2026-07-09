using MQContract.CQRS.Interfaces.Command;
using MQContract.CQRS.Interfaces.Query;
using System.Diagnostics;

namespace MQContract.CQRS.Interfaces;

/// <summary>
/// Defines an invocation context for a given message and or query instance
/// </summary>
public interface IInvocationContext
{
    /// <summary>
    /// The unique id for the given message
    /// </summary>
    Guid MessageId { get; }
    /// <summary>
    /// The unique id for the given message chain
    /// </summary>
    Guid CorrelationId { get; }
    /// <summary>
    /// The id of a source message if there is one through call chaining
    /// </summary>
    Guid? CausationId { get; }
    /// <summary>
    /// Used to set and get context specific data (current invocation context)
    /// </summary>
    /// <param name="key">The key for the data to retrieve</param>
    /// <returns>The stored context value for the given key or null when missing</returns>
    string? this[string key] { get; set; }
    /// <summary>
    /// The available keys that exist in this context
    /// </summary>
    IEnumerable<string> Keys { get; }
    /// <summary>
    /// The underlying activity used with OTEL.  This will be set if the underlying Contract Connection being used had OTEL enabled.
    /// </summary>
    Activity? Activity { get; }
    /// <summary>
    /// Called to execute a command of the given type through the current context
    /// </summary>
    /// <typeparam name="TCommand">The type of command to execute</typeparam>
    /// <param name="command">The command to execute</param>
    /// <returns>A ValueTask to allow for async execution</returns>
    /// <remarks>
    /// Tags: 
    ///     mqcontract.cqrs.correlationid = CorrelationId
    ///     mqcontract.cqrs.messageid = MessageId
    ///     mqcontract.cqrs.causationid = CausationId
    ///     mqcontract.cqrs.type = CQRS type (query or command)
    /// </remarks>
    ValueTask ExecuteCommandAsync<TCommand>(TCommand command)
        where TCommand : ICommand;
    /// <summary>
    /// Called to execute a command of the given type that is expected to provide a given response through the current context
    /// </summary>
    /// <typeparam name="TCommand">The type of command to execute</typeparam>
    /// <typeparam name="TCommandResult">The type of response to expect</typeparam>
    /// <param name="command">The command to execute</param>
    /// <param name="timeout">The timeout to allow for the execution, if not specified the underlying contract connection defaults will apply</param>
    /// <returns>The exepected result type</returns>
    ValueTask<TCommandResult?> ExecuteCommandAsync<TCommand, TCommandResult>(TCommand command, TimeSpan? timeout = null)
        where TCommand : ICommand<TCommandResult>;
    /// <summary>
    /// Called to execute a query of the given type through the current context
    /// </summary>
    /// <typeparam name="TQuery">The type of query to execute</typeparam>
    /// <typeparam name="TQueryResponse">The type of response to expect</typeparam>
    /// <param name="query">The query to execute</param>
    /// <param name="timeout">The timeout to allow for the execution, if not specified the underlying contract connection defaults will apply</param>
    /// <returns>The exepected result type</returns>
    ValueTask<TQueryResponse?> ExecuteQueryAsync<TQuery, TQueryResponse>(TQuery query, TimeSpan? timeout = null)
        where TQuery : IQuery;
}
