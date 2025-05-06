using MQContract.Interfaces;
using MQContract.Messages;
using System.Collections.Concurrent;

namespace MQContract.Connections
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
    internal abstract partial class AConnection<CC> : IMetricContractConnection<CC>
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
        where CC : IBaseContractConnection
    {
        private readonly ConcurrentDictionary<object, ResilliancePolicy> resilliancePolicies = [];
        private ResilliancePolicy defaultResilliancePolicy;

        private ResilliancePolicy BuildPolicy((int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
        {
            if (retryPolicy==null && circuitBreakPolicy == null)
                throw new InvalidPolicyArgumentsException([nameof(retryPolicy),nameof(circuitBreakPolicy)]);
            if (retryPolicy is not null && circuitBreakPolicy is not null && retryPolicy.Value.retryCount>circuitBreakPolicy.Value.handledEventsAllowedBeforeBreaking)
                throw new InvalidRetryCircuitBreakTriggersException(nameof(retryPolicy.Value.retryCount),nameof(circuitBreakPolicy.Value.handledEventsAllowedBeforeBreaking));
            return new(logger, retryPolicy, circuitBreakPolicy);
        }

        void IResillientContractConnection.RegisterResiliencePolicy((int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => defaultResilliancePolicy = BuildPolicy(retryPolicy, circuitBreakPolicy);

        void IResillientContractConnection.RegisterResiliencePolicy<T>((int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => resilliancePolicies.TryAdd(typeof(T), BuildPolicy(retryPolicy, circuitBreakPolicy));

        void IResillientContractConnection.RegisterResiliencePolicy(Type messageType, (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => resilliancePolicies.TryAdd(messageType, BuildPolicy(retryPolicy, circuitBreakPolicy));

        void IResillientContractConnection.RegisterResiliencePolicy(string messageChannel, (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => resilliancePolicies.TryAdd(messageChannel, BuildPolicy(retryPolicy, circuitBreakPolicy));

        protected async ValueTask<TransmissionResult> ExecuteResilliantTransmissionAsync<T>(Func<CancellationToken, ValueTask<TransmissionResult>> func, string channel, CancellationToken cancellationToken)
        {
            if (!resilliancePolicies.TryGetValue(channel, out var policy)
                && !resilliancePolicies.TryGetValue(typeof(T), out policy))
                    policy = defaultResilliancePolicy;
            if (policy==null)
                return await func(cancellationToken);
            return await policy.ExecuteResilliantTransmissionAsync(func, cancellationToken);
        }

        protected async ValueTask<QueryResult<T>> ExecuteResilliantTransmissionAsync<T>(Func<CancellationToken, ValueTask<QueryResult<T>>> func, string channel, CancellationToken cancellationToken)
        {
            if (!resilliancePolicies.TryGetValue(channel, out var policy)
                && !resilliancePolicies.TryGetValue(typeof(T), out policy))
                policy = defaultResilliancePolicy;
            if (policy==null)
                return await func(cancellationToken);
            return await policy.ExecuteResilliantTransmissionAsync<T>(func, cancellationToken);
        }

        protected async ValueTask<IEnumerable<TransmissionResult>> ExecuteResilliantTransmissionAsync<T>(Func<IEnumerable<ServiceMessage>, CancellationToken, ValueTask<IEnumerable<TransmissionResult>>> func, IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
        {
            if (!resilliancePolicies.TryGetValue(messages.First().Channel, out var policy)
                && !resilliancePolicies.TryGetValue(typeof(T), out policy))
                policy = defaultResilliancePolicy;
            if (policy == null)
                return await func(messages, cancellationToken);
            return await policy.ExecuteResilliantTransmissionAsync(func, messages, cancellationToken);
        }
    }
}
