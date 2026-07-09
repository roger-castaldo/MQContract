using MQContract.Interfaces;
using MQContract.Messages;
using System.Diagnostics.CodeAnalysis;

namespace MQContract;

internal partial class MessageContext
{
    public ValueTask<QueryResult<object>> ExecuteQuery<TQuery>(IContractConnection contractConnection, TransmissionMessage<TQuery> message, TimeSpan? timeout, string? channel, string? responseChannel, CancellationToken cancellationToken)
    {
        var resultTask = contexts.FirstOrDefault(context => context.IsMessageCodeGenerated<TQuery>())?
            .TryExecuteQuery<TQuery>(contractConnection, message!, timeout, channel, responseChannel, cancellationToken);
        if (resultTask.HasValue)
            return resultTask.Value;
        DynamicCodeNotSupportedException.ThrowIfDynamicCodeIsBlocked("Unable to execute query due to dynamic code not supported and query type not defined in context");
        return ExecuteQueryThroughReflection<TQuery>(contractConnection, message, timeout, channel, responseChannel, cancellationToken);
    }

    public ValueTask<IEnumerable<QueryResult<object>>> ExecuteQuery<TQuery>(IMultiServiceContractConnection contractConnection, TransmissionMessage<TQuery> message, TimeSpan? timeout, string? channel, string? responseChannel, CancellationToken cancellationToken)
    {
        var resultTask = contexts.FirstOrDefault(context => context.IsMessageCodeGenerated<TQuery>())?
            .TryExecuteQuery<TQuery>(contractConnection, message!, timeout, channel, responseChannel, cancellationToken);
        if (resultTask.HasValue)
            return resultTask.Value;
        DynamicCodeNotSupportedException.ThrowIfDynamicCodeIsBlocked("Unable to execute query due to dynamic code not supported and query type not defined in context");
        return ExecuteQueryThroughReflection<TQuery>(contractConnection, message, timeout, channel, responseChannel, cancellationToken);
    }

    private static QueryResult<object>? ConvertResultFromObject(object? obj)
    {
        if (obj == null) return null;
        var type = obj.GetType();
        if (type.IsGenericType && Equals(type.GetGenericTypeDefinition(), typeof(QueryResult<>)))
        {
            return new(
                (string)type.GetProperty(nameof(QueryResult<object>.ID))!.GetValue(obj)!,
                (MessageHeader)type.GetProperty(nameof(QueryResult<object>.Header))!.GetValue(obj)!,
                type.GetProperty(nameof(QueryResult<object>.Result))!.GetValue(obj),
                (ErrorMessage?)type.GetProperty(nameof(QueryResult<object>.Error))!.GetValue(obj)
            );
        }
        throw new InvalidCastException();
    }

    [RequiresUnreferencedCode("Uses reflection over generic methods and runtime types.")]
    private async ValueTask<QueryResult<object>> ExecuteQueryThroughReflection<TQuery>(IContractConnection contractConnection, TransmissionMessage<TQuery> message, TimeSpan? timeout, string? channel, string? responseChannel, CancellationToken cancellationToken)
    {
        var responseType = QueryResponseType<TQuery>()??throw new UnknownResponseTypeException("ResponseType", typeof(TQuery));
        try
        {
            return ConvertResultFromObject(await Utility.InvokeMethodAsync(
                    typeof(IContractConnection).GetMethods().First(m => Equals(m.Name, nameof(IContractConnection.QueryAsync))
                    && m.GetGenericArguments().Length==2
                    && m.GetParameters()[0].ParameterType.IsGenericType
                    && Equals(m.GetParameters()[0].ParameterType.GetGenericTypeDefinition(),typeof(TransmissionMessage<>)))
                    .MakeGenericMethod(typeof(TQuery), responseType!),
                    contractConnection,
                    [
                        message,
                        timeout,
                        channel,
                        responseChannel,
                        cancellationToken
                    ])
                )!;
        }
        catch (TimeoutException)
        {
            throw new QueryTimeoutException();
        }
    }

    [RequiresUnreferencedCode("Uses reflection over generic methods and runtime types.")]
    private async ValueTask<IEnumerable<QueryResult<object>>> ExecuteQueryThroughReflection<TQuery>(IMultiServiceContractConnection contractConnection, TransmissionMessage<TQuery> message, TimeSpan? timeout, string? channel, string? responseChannel, CancellationToken cancellationToken)
    {
        var responseType = QueryResponseType<TQuery>()??throw new UnknownResponseTypeException("ResponseType", typeof(TQuery));
        IEnumerable<object> results;
        try
        {
            results = (IEnumerable<object>)(await Utility.InvokeMethodAsync(
                typeof(IMultiServiceContractConnection).GetMethods()
        .First(method => Equals(method.Name, nameof(IMultiServiceContractConnection.QueryAsync)) && method.GetGenericArguments().Length==2
                    && method.GetParameters()[0].ParameterType.IsGenericType
                    && Equals(method.GetParameters()[0].ParameterType.GetGenericTypeDefinition(),typeof(TransmissionMessage<>)))
                    .MakeGenericMethod(typeof(TQuery), responseType!),
                contractConnection,
                [
                    message,
                    timeout,
                    channel,
                    responseChannel,
                    cancellationToken
                ])
            )!;
        }
        catch (TimeoutException)
        {
            throw new QueryTimeoutException();
        }
        return results.Select(obj => ConvertResultFromObject(obj)!);
    }
}
