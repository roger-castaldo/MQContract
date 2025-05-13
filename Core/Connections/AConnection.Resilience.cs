using MQContract.Interfaces;
using MQContract.Messages;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Channels;

namespace MQContract.Connections
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
    internal abstract partial class AConnection<CC> : IMetricContractConnection<CC>
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
        where CC : IBaseContractConnection
    {
        private readonly ConcurrentDictionary<Tuple<string?,object?>, ResiliencePolicy> resilliancePolicies = [];

        private ResiliencePolicy BuildPolicy((int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
        {
            if (retryPolicy==null && circuitBreakPolicy == null)
                throw new InvalidPolicyArgumentsException([nameof(retryPolicy), nameof(circuitBreakPolicy)]);
            if (retryPolicy is not null && circuitBreakPolicy is not null && retryPolicy.Value.retryCount>circuitBreakPolicy.Value.handledEventsAllowedBeforeBreaking)
                throw new InvalidRetryCircuitBreakTriggersException(nameof(retryPolicy.Value.retryCount), nameof(circuitBreakPolicy.Value.handledEventsAllowedBeforeBreaking));
            return new(logger, retryPolicy, circuitBreakPolicy);
        }

        protected CC AddPolicy(string? connectionName, object? key, (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
        {
            resilliancePolicies.TryAdd(new(connectionName,key), BuildPolicy(retryPolicy, circuitBreakPolicy));
            return (CC)(IBaseContractConnection)this;
        }

        CC IResilientContractConnection<CC>.RegisterResiliencePolicy((int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => AddPolicy(null, null, retryPolicy, circuitBreakPolicy);

        CC IResilientContractConnection<CC>.RegisterResiliencePolicy<T>((int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => AddPolicy(null,typeof(T), retryPolicy, circuitBreakPolicy);

        CC IResilientContractConnection<CC>.RegisterResiliencePolicy(Type messageType, (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => AddPolicy(null,messageType, retryPolicy, circuitBreakPolicy);

        CC IResilientContractConnection<CC>.RegisterResiliencePolicy(string messageChannel, (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => AddPolicy(null,messageChannel, retryPolicy, circuitBreakPolicy);

        private ResiliencePolicy? GetResilliancePolicy<T>(string? connectionName,string channel)
        {
            ResiliencePolicy? policy = null;
            if (connectionName!=null
                &&!resilliancePolicies.TryGetValue(new(connectionName, channel), out policy)
                && !resilliancePolicies.TryGetValue(new(connectionName, typeof(T)), out policy))
                resilliancePolicies.TryGetValue(new(connectionName, null), out policy);
            if (policy==null 
                && !resilliancePolicies.TryGetValue(new(null, channel), out policy)
                && !resilliancePolicies.TryGetValue(new(null, typeof(T)), out policy))
                resilliancePolicies.TryGetValue(new(null, null),out policy);
            return policy;
        }

        protected async ValueTask<TransmissionResult> ExecuteResilliantTransmissionAsync<T>(Func<CancellationToken, ValueTask<TransmissionResult>> func, string? connectionName, string channel, CancellationToken cancellationToken)
        {
            var policy = GetResilliancePolicy<T>(connectionName, channel);
            if (policy==null)
                return await func(cancellationToken);
            return await policy.ExecuteResilliantTransmissionAsync(func, cancellationToken);
        }

        protected async ValueTask<QueryResult<R>> ExecuteResilliantTransmissionAsync<Q, R>(Func<CancellationToken, ValueTask<QueryResult<R>>> func, string? connectionName, string channel, CancellationToken cancellationToken)
        {
            var policy = GetResilliancePolicy<Q>(connectionName, channel);
            if (policy==null)
                return await func(cancellationToken);
            return await policy.ExecuteResilliantTransmissionAsync<R>(func, cancellationToken);
        }

        protected async ValueTask<IEnumerable<TransmissionResult>> ExecuteResilliantTransmissionAsync<T>(Func<IEnumerable<ServiceMessage>, CancellationToken, ValueTask<IEnumerable<TransmissionResult>>> func, string? connectionName, IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
        {
            var policy = GetResilliancePolicy<T>(connectionName, messages.First().Channel);
            if (policy == null)
                return await func(messages, cancellationToken);
            return await policy.ExecuteResilliantTransmissionAsync(func, messages, cancellationToken);
        }
    }
}
