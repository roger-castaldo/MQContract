using Microsoft.Extensions.Logging;
using MQContract.Extensions;
using MQContract.Messages;
using Polly;
using Polly.CircuitBreaker;
using System.Collections.Concurrent;

namespace MQContract.Connections
{
    internal class ResiliencePolicy(ILogger? logger,
        (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy,
        (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy
    )
    {
        private static IEnumerable<TResult> WrapInstance<TResult>(ResilienceTypes type, IEnumerable<TResult> instances)
            where TResult : TransmissionResult
        {
            var constructor = typeof(TResult).GetConstructors()[0];
            var parameters = constructor.GetParameters();
            return instances.Select(instance =>
            {
                var arguments = parameters.Select(p =>
                {
                    var value = typeof(TResult).GetProperty(p.Name!)!.GetValue(instance);
                    if (value is ErrorMessage errorMessage)
                    {
                        return new ErrorMessage(new ResilienceException(type, errorMessage.Exception), errorMessage.IsFatal);
                    }
                    return value;
                }).ToArray();
                return (TResult)constructor.Invoke(arguments);
            });
        }

        private static AsyncPolicy<IEnumerable<TResult>> BuildPolicy<TResult>(ILogger? logger,
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy
        )
            where TResult : TransmissionResult
        {
            AsyncPolicy<IEnumerable<TResult>>? fallbackPolicy = null;
            AsyncPolicy<IEnumerable<TResult>>? retry = null;
            AsyncPolicy<IEnumerable<TResult>>? circuit = null;
            if (retryPolicy!=null)
            {
                fallbackPolicy = Policy
                        .HandleResult<IEnumerable<TResult>>(results => results.Any(r => r.IsError && !r.Error!.IsFatal))
                        .FallbackAsync<IEnumerable<TResult>>(
                            fallbackAction: (delegateResult, context, cancellationToken) =>
                            {
                                foreach (var result in delegateResult.Result.Where(r => r.IsError))
                                    logger?.LogDebugChecked("Retry fallback has been triggered for {MessageID}", result.ID);
                                return Task.FromResult(WrapInstance<TResult>(ResilienceTypes.Retry, delegateResult.Result));
                            },
                            onFallbackAsync: (delegateResult, cancellationToken) =>
                            {
                                logger?.LogErrorChecked(delegateResult.Exception, "Failed to fallback");
                                return Task.CompletedTask;
                            }
                        );
                retry = Policy
                      .HandleResult<IEnumerable<TResult>>(results => results.Any(result => result.IsError && !result.Error!.IsFatal))
                      .WaitAndRetryAsync(retryPolicy.Value.retryCount, retryPolicy.Value.sleepDurationProvider);
            }
            if (circuitBreakPolicy!=null)
            {
                circuit = Policy
                        .HandleResult<IEnumerable<TResult>>(results => results.Any(r => r.IsError && !r.Error!.IsFatal))
                        .CircuitBreakerAsync(circuitBreakPolicy.Value.handledEventsAllowedBeforeBreaking, circuitBreakPolicy.Value.durationOfBreak);
            }
            return (retry, circuit) switch
            {
                (not null, not null) => fallbackPolicy!.WrapAsync(retry!.WrapAsync(circuit)),
                (not null, null) => fallbackPolicy!.WrapAsync(retry!),
                _ => circuit!
            };
        }

        private const string SuccessStorageKey = "SuccessfulMessages";
        private const string ServiceMessagesKey = "ServiceMessages";

        private readonly AsyncPolicy<IEnumerable<TransmissionResult>> transmissionPolicy = BuildPolicy<TransmissionResult>(logger, retryPolicy, circuitBreakPolicy);
        private readonly ConcurrentDictionary<Type, object> queryPolicies = [];

        public async ValueTask<TransmissionResult> ExecuteResilliantTransmissionAsync(Func<CancellationToken, ValueTask<TransmissionResult>> func, CancellationToken cancellationToken)
        {
            try
            {
                return (await transmissionPolicy.ExecuteAsync(async (ct) => [await func(ct)], cancellationToken)).First();
            }
            catch (BrokenCircuitException bce)
            {
                return new(string.Empty, new(new ResilienceException(ResilienceTypes.CircuitBreak, bce)));
            }
        }


        public async ValueTask<IEnumerable<TransmissionResult>> ExecuteResilliantTransmissionAsync(Func<IEnumerable<ServiceMessage>, CancellationToken, ValueTask<IEnumerable<TransmissionResult>>> func, IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
        {
            var resultContext = new Context();
            resultContext.Add(ServiceMessagesKey, messages.ToArray());

            try
            {
                return await transmissionPolicy.ExecuteAsync(async (context, cancellation) =>
                {
                    IEnumerable<TransmissionResult> currentSuccess = (context.ContainsKey(SuccessStorageKey) ? (IEnumerable<TransmissionResult>)context[SuccessStorageKey] : []);
                    var serviceMessages = (ServiceMessage[])context[ServiceMessagesKey];
                    var response = await func(serviceMessages.Where(msg => !currentSuccess.Any(res => Equals(msg.ID, res.ID))), cancellation);
                    context.Remove(SuccessStorageKey);
                    context.Add(SuccessStorageKey, currentSuccess.Concat(response.Where(resp => !resp.IsError || (resp.IsError && resp.Error!.IsFatal))).ToArray());
                    return currentSuccess.Concat(response)
                        .OrderBy(rep => Array.FindIndex(serviceMessages, msg => Equals(msg.ID, rep.ID)))
                        .ToArray();
                }, resultContext, cancellationToken);
            }
            catch (BrokenCircuitException bce)
            {
                return messages.Select(msg => new TransmissionResult(msg.ID, new(new ResilienceException(ResilienceTypes.CircuitBreak, bce))));
            }
        }

        public async ValueTask<QueryResult<TQueryResult>> ExecuteResilliantTransmissionAsync<TQueryResult>(Func<CancellationToken, ValueTask<QueryResult<TQueryResult>>> func, CancellationToken cancellationToken)
        {
            if (!queryPolicies.TryGetValue(typeof(TQueryResult), out var policy))
            {
                policy = BuildPolicy<QueryResult<TQueryResult>>(logger, retryPolicy, circuitBreakPolicy);
                queryPolicies.TryAdd(typeof(TQueryResult), policy);
            }
            try
            {
                return (await ((AsyncPolicy<IEnumerable<QueryResult<TQueryResult>>>)policy).ExecuteAsync(async (ct) => [await func(ct)], cancellationToken)).First();
            }
            catch (BrokenCircuitException bce)
            {
                return new(string.Empty, new([]), Error: new(new ResilienceException(ResilienceTypes.CircuitBreak, bce)));
            }
        }
    }
}
