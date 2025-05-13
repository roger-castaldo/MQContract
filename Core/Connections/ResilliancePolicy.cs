using Microsoft.Extensions.Logging;
using MQContract.Messages;
using Polly;
using Polly.CircuitBreaker;
using System.Collections.Concurrent;

namespace MQContract.Connections
{
    internal class ResilliancePolicy(ILogger? logger,
        (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, 
        (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy
    )
    {
        private static IEnumerable<T> WrapInstance<T>(ResillianceTypes type, IEnumerable<T> instances)
            where T : TransmissionResult
        {
            var constructor = typeof(T).GetConstructors()[0];
            var parameters = constructor.GetParameters();
            return instances.Select(instance => { 
                var arguments = parameters.Select(p => {
                    var value = typeof(T).GetProperty(p.Name!)!.GetValue(instance);
                    if (value is ErrorMessage errorMessage)
                    {
                        return new ErrorMessage(new ResillianceException(type, errorMessage.Exception), errorMessage.IsFatal);
                    }
                    return value;
                }).ToArray();
                return (T)constructor.Invoke(arguments);
            });
        }

        private static AsyncPolicy<IEnumerable<T>> BuildPolicy<T>(ILogger? logger,
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy
        )
            where T : TransmissionResult
        {
            AsyncPolicy<IEnumerable<T>>? fallbackPolicy = null;
            AsyncPolicy<IEnumerable<T>>? retry = null;
            AsyncPolicy<IEnumerable<T>>? circuit = null;
            if (retryPolicy!=null)
            {
                fallbackPolicy = Policy
                        .HandleResult<IEnumerable<T>>(results => results.Any(r => r.IsError && !r.Error!.IsFatal))
                        .FallbackAsync<IEnumerable<T>>(
                            fallbackAction: (delegateResult, context, cancellationToken) =>
                            {
                                foreach (var result in delegateResult.Result.Where(r => r.IsError))
                                    logger?.LogDebug("Retry fallback has been triggered for {MessageID}", result.ID);
                                return Task.FromResult(WrapInstance<T>(ResillianceTypes.Retry, delegateResult.Result));
                            },
                            onFallbackAsync: (delegateResult, cancellationToken) =>
                            {
                                logger?.LogError(delegateResult.Exception, "Failed to fallback");
                                return Task.CompletedTask;
                            }
                        );
                retry = Policy                    
                      .HandleResult<IEnumerable<T>>(results => results.Any(result => result.IsError && !result.Error!.IsFatal))
                      .WaitAndRetryAsync(retryPolicy.Value.retryCount, retryPolicy.Value.sleepDurationProvider);
            }
            if (circuitBreakPolicy!=null)
            {
                circuit = Policy
                        .HandleResult<IEnumerable<T>>(results => results.Any(r => r.IsError && !r.Error!.IsFatal))
                        .CircuitBreakerAsync(circuitBreakPolicy.Value.handledEventsAllowedBeforeBreaking, circuitBreakPolicy.Value.durationOfBreak);
            }
            return (retry, circuit) switch
            {
                (not null, not null) => fallbackPolicy!.WrapAsync(retry.WrapAsync(circuit)),
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
                return new(string.Empty, new(new ResillianceException(ResillianceTypes.CircuitBreak, bce)));
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
                    var response = await func(serviceMessages.Where(msg=>!currentSuccess.Any(res=>Equals(msg.ID,res.ID))), cancellation);
                    context.Remove(SuccessStorageKey);
                    context.Add(SuccessStorageKey, currentSuccess.Concat(response.Where(resp => !resp.IsError || (resp.IsError && resp.Error!.IsFatal))).ToArray());
                    return currentSuccess.Concat(response)
                        .OrderBy(rep => Array.FindIndex(serviceMessages, msg => Equals(msg.ID, rep.ID)))
                        .ToArray();
                }, resultContext, cancellationToken);
            }
            catch (BrokenCircuitException bce)
            {
                return messages.Select(msg => new TransmissionResult(msg.ID, new(new ResillianceException(ResillianceTypes.CircuitBreak, bce))));
            }
        }

        public async ValueTask<QueryResult<T>> ExecuteResilliantTransmissionAsync<T>(Func<CancellationToken, ValueTask<QueryResult<T>>> func, CancellationToken cancellationToken)
        {
            if (!queryPolicies.TryGetValue(typeof(T),out var policy))
            {
                policy = BuildPolicy<QueryResult<T>>(logger, retryPolicy, circuitBreakPolicy);
                queryPolicies.TryAdd(typeof(T), policy);
            }
            try
            {
                return (await ((AsyncPolicy<IEnumerable<QueryResult<T>>>)policy).ExecuteAsync(async (ct) => [await func(ct)], cancellationToken)).First();
            }
            catch (BrokenCircuitException bce)
            {
                return new(string.Empty, new([]), Error: new(new ResillianceException(ResillianceTypes.CircuitBreak, bce)));
            }
        }
    }
}
