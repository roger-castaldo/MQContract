using Microsoft.Extensions.Logging;
using MQContract.Loggers;
using MQContract.Messages;
using MQContract.Middleware;
using Polly;
using Polly.CircuitBreaker;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MQContract.Connections
{
    internal class ResiliencePolicy(string name,
        ILogger logger,
        (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy,
        (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy
    )
    {
        private const string ActivityContextKey = "Activity";
        private static readonly string PolicyNameTag = $"{OpenTelemetryMiddleware.KeyBase}.resiliencepolicyname";

        private static AsyncPolicy<IEnumerable<TResult>>? BuildRetryPolicy<TResult>((int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy)
            where TResult : TransmissionResult
            => (retryPolicy==null ?
                null :
                Policy.HandleResult<IEnumerable<TResult>>(results => results.Any(result => result.IsError && !result.Error!.IsFatal))
                      .WaitAndRetryAsync(retryPolicy.Value.retryCount, retryPolicy.Value.sleepDurationProvider, (outcome, timeSpan, retryAttempt, context) =>
                      {
                          var activity = (Activity?)context[ActivityContextKey];
                          foreach (var result in outcome.Result.Where(r => r.IsError && !r.Error!.IsFatal))
                          {
                              activity?.AddEvent(new(
                                  "Resilliant Retry Triggered",
                                  tags: new([
                                      new(OpenTelemetryMiddleware.MessageIdKey,result.ID),
                                      new($"{OpenTelemetryMiddleware.KeyBase}.resulttype",typeof(TResult).Name),
                                      new($"{OpenTelemetryMiddleware.KeyBase}.retryattempt",retryAttempt)
                                  ])
                              ));
                          }
                      })
            );

        private static AsyncPolicy<IEnumerable<TResult>>? BuildCircuitBreakPolicy<TResult>(string name, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            where TResult : TransmissionResult
            => (circuitBreakPolicy==null ?
                null :
                Policy.HandleResult<IEnumerable<TResult>>(results => results.Any(r => r.IsError && !r.Error!.IsFatal))
                    .CircuitBreakerAsync(circuitBreakPolicy.Value.handledEventsAllowedBeforeBreaking, circuitBreakPolicy.Value.durationOfBreak, (outcome, timespan, context) =>
                    {
                        foreach (var result in outcome.Result.Where(r => r.IsError && !r.Error!.IsFatal))
                        {
                            ((Activity?)context[ActivityContextKey])?.AddEvent(new(
                                "Resilliant Circuit Breaker Triggered",
                                tags: new([
                                    new(OpenTelemetryMiddleware.MessageIdKey,result.ID),
                                      new($"{OpenTelemetryMiddleware.KeyBase}.resulttype",typeof(TResult).Name)
                                ])
                            ));
                        }
                    }, (context) =>
                    {
                        ((Activity?)context[ActivityContextKey])?.AddEvent(new(
                                "Resilliant Circuit Breaker Cleared",
                                tags: new([
                                    new(PolicyNameTag, name),
                                    new($"{OpenTelemetryMiddleware.KeyBase}.resulttype",typeof(TResult).Name)
                                ])
                            ));
                    })
            );

        private static AsyncPolicy<IEnumerable<TransmissionResult>> BuildTransmissionPolicy(
            string name,
            ILogger logger,
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy
        )
        {
            AsyncPolicy<IEnumerable<TransmissionResult>>? fallbackPolicy = null;
            AsyncPolicy<IEnumerable<TransmissionResult>>? retry = BuildRetryPolicy<TransmissionResult>(retryPolicy);
            AsyncPolicy<IEnumerable<TransmissionResult>>? circuit = BuildCircuitBreakPolicy<TransmissionResult>(name, circuitBreakPolicy);
            if (retry !=null)
            {
                fallbackPolicy = Policy
                        .HandleResult<IEnumerable<TransmissionResult>>(results => results.Any(r => r.IsError && !r.Error!.IsFatal))
                        .FallbackAsync(
                            fallbackAction: (delegateResult, context, cancellationToken) =>
                            {
                                foreach (var result in delegateResult.Result.Where(r => r.IsError))
                                    BaseLog.ResilienceRetryTriggered(logger, result.ID);
                                return Task.FromResult(delegateResult.Result.Select(instance => new TransmissionResult(instance.ID, (instance.IsError ? new ErrorMessage(new ResilienceException(ResilienceTypes.Retry, instance.Error!.Exception)) : null))));
                            },
                            onFallbackAsync: (delegateResult, cancellationToken) =>
                            {
                                BaseLog.ResilienceFailedToFallback(logger, delegateResult.Exception);
                                return Task.CompletedTask;
                            }
                        );
            }
            return (retry, circuit) switch
            {
                (not null, not null) => fallbackPolicy!.WrapAsync(retry!.WrapAsync(circuit)),
                (not null, null) => fallbackPolicy!.WrapAsync(retry!),
                _ => circuit!
            };
        }

        private static AsyncPolicy<IEnumerable<QueryResult<TQueryResult>>> BuildQueryPolicy<TQueryResult>(string name, ILogger logger,
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy
        )
        {
            AsyncPolicy<IEnumerable<QueryResult<TQueryResult>>>? fallbackPolicy = null;
            AsyncPolicy<IEnumerable<QueryResult<TQueryResult>>>? retry = BuildRetryPolicy<QueryResult<TQueryResult>>(retryPolicy);
            AsyncPolicy<IEnumerable<QueryResult<TQueryResult>>>? circuit = BuildCircuitBreakPolicy<QueryResult<TQueryResult>>(name, circuitBreakPolicy);
            if (retry !=null)
            {
                fallbackPolicy = Policy
                        .HandleResult<IEnumerable<QueryResult<TQueryResult>>>(results => results.Any(r => r.IsError && !r.Error!.IsFatal))
                        .FallbackAsync(
                            fallbackAction: (delegateResult, context, cancellationToken) =>
                            {
                                foreach (var result in delegateResult.Result.Where(r => r.IsError))
                                    BaseLog.ResilienceRetryTriggered(logger, result.ID);
                                return Task.FromResult(delegateResult.Result.Select(instance => new QueryResult<TQueryResult>(instance.ID, instance.Header, instance.Result, (instance.IsError ? new ErrorMessage(new ResilienceException(ResilienceTypes.Retry, instance.Error!.Exception)) : null))));
                            },
                            onFallbackAsync: (delegateResult, cancellationToken) =>
                            {
                                BaseLog.ResilienceFailedToFallback(logger, delegateResult.Exception);
                                return Task.CompletedTask;
                            }
                        );
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

        private readonly AsyncPolicy<IEnumerable<TransmissionResult>> transmissionPolicy = BuildTransmissionPolicy(name, logger, retryPolicy, circuitBreakPolicy);
        private readonly ConcurrentDictionary<Type, object> queryPolicies = [];

        private void AddExecutingEvent(Activity? current)
            => current?.AddEvent(new(
                    "Executing Resilience Policy",
                    tags: new([
                        new(PolicyNameTag,name)
                    ])
                ));

        public async ValueTask<TransmissionResult> ExecuteResilliantTransmissionAsync(Activity? activity, Func<CancellationToken, ValueTask<TransmissionResult>> func, CancellationToken cancellationToken)
        {
            AddExecutingEvent(activity);
            var resultContext = new Polly.Context
            {
                { ActivityContextKey, activity }
            };
            try
            {
                return (await transmissionPolicy.ExecuteAsync(async (context, cancellation) => [await func(cancellation)], resultContext, cancellationToken)).First();
            }
            catch (BrokenCircuitException bce)
            {
                return new(string.Empty, new(new ResilienceException(ResilienceTypes.CircuitBreak, bce)));
            }
        }

        public async ValueTask<IEnumerable<TransmissionResult>> ExecuteResilliantTransmissionAsync(Activity? activity, Func<IEnumerable<ServiceMessage>, CancellationToken, ValueTask<IEnumerable<TransmissionResult>>> func, IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
        {
            AddExecutingEvent(activity);
            var resultContext = new Polly.Context
            {
                { ActivityContextKey, activity },
                {ServiceMessagesKey, messages.ToArray() }
            };

            try
            {
                return await transmissionPolicy.ExecuteAsync(async (context, cancellation) =>
                {
                    IEnumerable<TransmissionResult> currentSuccess = (context.ContainsKey(SuccessStorageKey) ? (IEnumerable<TransmissionResult>)context[SuccessStorageKey] : []);
                    var serviceMessages = (ServiceMessage[])context[ServiceMessagesKey];
                    var response = await func(serviceMessages.Where(msg => !currentSuccess.Any(res => Equals(msg.ID, res.ID))), cancellation);
                    context.Remove(SuccessStorageKey);
                    context.Add(SuccessStorageKey, currentSuccess.Concat(response.Where(resp => !resp.IsError || (resp.IsError && resp.Error!.IsFatal))).ToArray());
                    return [.. currentSuccess.Concat(response).OrderBy(rep => Array.FindIndex(serviceMessages, msg => Equals(msg.ID, rep.ID)))];
                }, resultContext, cancellationToken);
            }
            catch (BrokenCircuitException bce)
            {
                return messages.Select(msg => new TransmissionResult(msg.ID, new(new ResilienceException(ResilienceTypes.CircuitBreak, bce))));
            }
        }

        public async ValueTask<QueryResult<TQueryResult>> ExecuteResilliantTransmissionAsync<TQueryResult>(Activity? activity, Func<CancellationToken, ValueTask<QueryResult<TQueryResult>>> func, CancellationToken cancellationToken)
        {
            if (!queryPolicies.TryGetValue(typeof(TQueryResult), out var policy))
            {
                policy = BuildQueryPolicy<TQueryResult>(name, logger, retryPolicy, circuitBreakPolicy);
                queryPolicies.TryAdd(typeof(TQueryResult), policy);
            }
            AddExecutingEvent(activity);
            var resultContext = new Polly.Context
            {
                { ActivityContextKey, activity }
            };
            try
            {
                return (await ((AsyncPolicy<IEnumerable<QueryResult<TQueryResult>>>)policy).ExecuteAsync(async (context, cancellation) => [await func(cancellation)], resultContext, cancellationToken)).First();
            }
            catch (BrokenCircuitException bce)
            {
                return new(string.Empty, new([]), Error: new(new ResilienceException(ResilienceTypes.CircuitBreak, bce)));
            }
        }
    }
}
