using MQContract.Interfaces;
using MQContract.Messages;
using System.Collections.Concurrent;

namespace MQContract.Connections
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
    internal abstract partial class AConnection<TContractConnection> : IMetricContractConnection<TContractConnection>
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
        where TContractConnection : IBaseContractConnection
    {
        private readonly ConcurrentDictionary<Tuple<string?, object?>, ResiliencePolicy> resilliancePolicies = [];

        private ResiliencePolicy BuildPolicy((int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
        {
            if (retryPolicy==null && circuitBreakPolicy == null)
                throw new InvalidPolicyArgumentsException([nameof(retryPolicy), nameof(circuitBreakPolicy)]);
            if (retryPolicy is not null && circuitBreakPolicy is not null && retryPolicy.Value.retryCount>circuitBreakPolicy.Value.handledEventsAllowedBeforeBreaking)
                throw new InvalidRetryCircuitBreakTriggersException(nameof(retryPolicy.Value.retryCount), nameof(circuitBreakPolicy.Value.handledEventsAllowedBeforeBreaking));
            return new(logger, retryPolicy, circuitBreakPolicy);
        }

        protected TContractConnection AddPolicy(string? connectionName, object? key, (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
        {
            resilliancePolicies.TryAdd(new(connectionName, key), BuildPolicy(retryPolicy, circuitBreakPolicy));
            return (TContractConnection)(IBaseContractConnection)this;
        }

        TContractConnection IResilientContractConnection<TContractConnection>.RegisterResiliencePolicy((int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => AddPolicy(null, null, retryPolicy, circuitBreakPolicy);

        TContractConnection IResilientContractConnection<TContractConnection>.RegisterResiliencePolicy<TMessage>((int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => AddPolicy(null, typeof(TMessage), retryPolicy, circuitBreakPolicy);

        TContractConnection IResilientContractConnection<TContractConnection>.RegisterResiliencePolicy(Type messageType, (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => AddPolicy(null, messageType, retryPolicy, circuitBreakPolicy);

        TContractConnection IResilientContractConnection<TContractConnection>.RegisterResiliencePolicy(string messageChannel, (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
            => AddPolicy(null, messageChannel, retryPolicy, circuitBreakPolicy);

        private ResiliencePolicy? GetResilliancePolicy<TMessage>(string? connectionName, string channel)
        {
            ResiliencePolicy? policy = null;
            if (connectionName!=null
                &&!resilliancePolicies.TryGetValue(new(connectionName, channel), out policy)
                && !resilliancePolicies.TryGetValue(new(connectionName, typeof(TMessage)), out policy))
                resilliancePolicies.TryGetValue(new(connectionName, null), out policy);
            if (policy==null
                && !resilliancePolicies.TryGetValue(new(null, channel), out policy)
                && !resilliancePolicies.TryGetValue(new(null, typeof(TMessage)), out policy))
                resilliancePolicies.TryGetValue(new(null, null), out policy);
            return policy;
        }

        protected async ValueTask<TransmissionResult> ExecuteResilliantTransmissionAsync<TMessage>(Func<CancellationToken, ValueTask<TransmissionResult>> func, string? connectionName, string channel, CancellationToken cancellationToken)
        {
            var policy = GetResilliancePolicy<TMessage>(connectionName, channel);
            if (policy==null)
                return await func(cancellationToken);
            return await policy.ExecuteResilliantTransmissionAsync(func, cancellationToken);
        }

        protected async ValueTask<QueryResult<TQueryResponse>> ExecuteResilliantTransmissionAsync<TQuery, TQueryResponse>(Func<CancellationToken, ValueTask<QueryResult<TQueryResponse>>> func, string? connectionName, string channel, CancellationToken cancellationToken)
        {
            var policy = GetResilliancePolicy<TQuery>(connectionName, channel);
            if (policy==null)
                return await func(cancellationToken);
            return await policy.ExecuteResilliantTransmissionAsync<TQueryResponse>(func, cancellationToken);
        }

        protected async ValueTask<IEnumerable<TransmissionResult>> ExecuteResilliantTransmissionAsync<TMessage>(Func<IEnumerable<ServiceMessage>, CancellationToken, ValueTask<IEnumerable<TransmissionResult>>> func, string? connectionName, IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
        {
            var policy = GetResilliancePolicy<TMessage>(connectionName, messages.First().Channel);
            if (policy == null)
                return await func(messages, cancellationToken);
            return await policy.ExecuteResilliantTransmissionAsync(func, messages, cancellationToken);
        }
    }
}
