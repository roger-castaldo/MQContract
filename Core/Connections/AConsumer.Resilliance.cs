using MQContract.Interfaces;
using MQContract.Messages;
using Polly;
using Polly.CircuitBreaker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly.Wrap;

namespace MQContract.Connections
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
    internal abstract partial class AConnection<CC> : IMetricContractConnection<CC>
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
        where CC : IBaseContractConnection
    {
        private readonly ConcurrentDictionary<object, AsyncPolicyWrap<TransmissionResult>> resilliancePolicies = [];
        private AsyncPolicyWrap<TransmissionResult>? defaultResilliancePolicy;

        private AsyncPolicyWrap<TransmissionResult> BuildPolicy((int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
        {
            if (retryPolicy==null && circuitBreakPolicy == null)
                throw new ArgumentNullException($"{nameof(retryPolicy)},{nameof(circuitBreakPolicy)}", "You must supply at least a retryPolicy or a circuitBreakPolicy");
            return Policy.WrapAsync<TransmissionResult>(
                Policy
                .HandleResult<TransmissionResult>(result => result.IsError && !result.Error!.IsFatal)
                .FallbackAsync<TransmissionResult>(
                    fallbackAction: (delegateResult, context, cancellationToken)
                        => Task.FromResult(delegateResult.Result)
                    ,
                    onFallbackAsync: (delegateResult, cancellationToken) =>
                    {
                        logger?.LogError(delegateResult.Exception, "Failed to fallback");
                        return Task.CompletedTask;
                    }
                ),
                Policy
                .HandleResult<TransmissionResult>(result => result.IsError && !result.Error!.IsFatal)
                .WaitAndRetryAsync(retryPolicy?.retryCount??0, retryPolicy?.sleepDurationProvider??((count) => TimeSpan.MinValue)),
                Policy
                .HandleResult<TransmissionResult>(result => result.IsError && !result.Error!.IsFatal)
                .CircuitBreakerAsync(circuitBreakPolicy?.handledEventsAllowedBeforeBreaking??1, circuitBreakPolicy?.durationOfBreak??TimeSpan.FromSeconds(1))
            );
        }

        private void RegisterPolicy(object key, (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => resilliancePolicies.TryAdd(key, BuildPolicy(retryPolicy, circuitBreakPolicy));

        void IResillientContractConnection.RegisterTransientPublishPolicy((int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => defaultResilliancePolicy = BuildPolicy(retryPolicy, circuitBreakPolicy);

        void IResillientContractConnection.RegisterTransientPublishPolicy<T>((int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => RegisterPolicy(typeof(T),retryPolicy, circuitBreakPolicy);

        void IResillientContractConnection.RegisterTransientPublishPolicy(Type messageType, (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => RegisterPolicy(messageType, retryPolicy, circuitBreakPolicy);

        void IResillientContractConnection.RegisterTransientPublishPolicy(string messageChannel, (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => RegisterPolicy(messageChannel, retryPolicy, circuitBreakPolicy);

        protected async ValueTask<TransmissionResult> ExecuteResilliantTransmissionAsync<T>(Func<CancellationToken, ValueTask<TransmissionResult>> func, string channel, CancellationToken cancellationToken)
        {
            if (!resilliancePolicies.TryGetValue(channel, out var policy)
                && !resilliancePolicies.TryGetValue(typeof(T), out policy))
                    policy = defaultResilliancePolicy;
            if (policy==null)
                return await func(cancellationToken);
            return await policy.ExecuteAsync(async (ct) => await func(ct), cancellationToken);
        }

        protected async ValueTask<QueryResult<T>> ExecuteResilliantTransmissionAsync<T>(Func<CancellationToken, ValueTask<QueryResult<T>>> func, string channel, CancellationToken cancellationToken)
        {
            if (!resilliancePolicies.TryGetValue(channel, out var policy)
                && !resilliancePolicies.TryGetValue(typeof(T), out policy))
                policy = defaultResilliancePolicy;
            if (policy==null)
                return await func(cancellationToken);
            return (QueryResult<T>)(await policy.ExecuteAsync(async (ct) => await func(ct), cancellationToken));
        }
    }
}
