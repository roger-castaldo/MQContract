using Microsoft.CodeAnalysis.CSharp.Syntax;
using MQContract.Interfaces;
using MQContract.Messages;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MQContract.Connections
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
    internal abstract partial class AConnection<TContractConnection> : IMetricContractConnection<TContractConnection>
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
        where TContractConnection : IBaseContractConnection
    {
        private readonly ConcurrentDictionary<(string? connectionName, object? dataType), ResiliencePolicy?> resilliancePolicies = [];

        private ResiliencePolicy BuildPolicy(string name, (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy, (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
        {
            if (retryPolicy==null && circuitBreakPolicy == null)
                throw new InvalidPolicyArgumentsException([nameof(retryPolicy), nameof(circuitBreakPolicy)]);
            if (retryPolicy is not null && circuitBreakPolicy is not null && retryPolicy.Value.retryCount>circuitBreakPolicy.Value.handledEventsAllowedBeforeBreaking)
                throw new InvalidRetryCircuitBreakTriggersException(nameof(retryPolicy.Value.retryCount), nameof(circuitBreakPolicy.Value.handledEventsAllowedBeforeBreaking));
            return new(name, Logger, retryPolicy, circuitBreakPolicy);
        }

        protected TContractConnection AddPolicy(
            string? connectionName,
            object? key,
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy)
        {
            var keyName = (key) switch
            {
                (var o) when o is Type t => $"messageType[{t.Name}]",
                (var o) when o is string s => $"messageChannel[{s}]",
                _ => "default"
            };
            resilliancePolicies.TryAdd((connectionName, key), BuildPolicy($"{(connectionName==null ? "" : $"{connectionName}-")}{keyName}", retryPolicy, circuitBreakPolicy));
            foreach(var k in resilliancePolicies.Keys
                .Where(static k =>!string.IsNullOrWhiteSpace(k.connectionName) && k.dataType is not null && k.dataType is not Type && k.dataType is not string)
                .ToArray())
                resilliancePolicies.TryRemove(k,out _);
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
            if (resilliancePolicies.IsEmpty)
                return null;

            var msgType = typeof(TMessage);

            if (resilliancePolicies.TryGetValue((connectionName, (channel, msgType)), out var policy))
                return policy;

            // Ordered lookup (most specific → least specific)
            var keys = new (string?, object?)[]
            {
                (connectionName, channel),
                (connectionName, msgType),
                (connectionName, null),
                (null, channel),
                (null, msgType),
                (null, null)
            };

            foreach (var key in keys)
            {
                if (resilliancePolicies.TryGetValue(key, out policy)) {
                    resilliancePolicies.TryAdd((connectionName, (channel, msgType)), policy);
                    return policy;
                }
            }

            resilliancePolicies.TryAdd((connectionName, (channel, msgType)), null);
            return null;
        }

        protected async ValueTask<TransmissionResult> ExecuteResilliantTransmissionAsync<TMessage>(Func<CancellationToken, ValueTask<TransmissionResult>> func, Activity? activity, string? connectionName, string channel, CancellationToken cancellationToken)
        {
            var policy = GetResilliancePolicy<TMessage>(connectionName, channel);
            if (policy==null)
                return await func(cancellationToken);
            return await policy.ExecuteResilliantTransmissionAsync(activity, func, cancellationToken);
        }

        protected async ValueTask<QueryResult<TQueryResponse>> ExecuteResilliantTransmissionAsync<TQuery, TQueryResponse>(Func<CancellationToken, ValueTask<QueryResult<TQueryResponse>>> func, Activity? activity, string? connectionName, string channel, CancellationToken cancellationToken)
        {
            var policy = GetResilliancePolicy<TQuery>(connectionName, channel);
            if (policy==null)
                return await func(cancellationToken);
            return await policy.ExecuteResilliantTransmissionAsync<TQueryResponse>(activity, func, cancellationToken);
        }

        protected async ValueTask<IEnumerable<TransmissionResult>> ExecuteResilliantTransmissionAsync<TMessage>(Func<IEnumerable<ServiceMessage>, CancellationToken, ValueTask<IEnumerable<TransmissionResult>>> func, Activity? activity, string? connectionName, IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
        {
            var policy = GetResilliancePolicy<TMessage>(connectionName, messages.First().Channel);
            if (policy == null)
                return await func(messages, cancellationToken);
            return await policy.ExecuteResilliantTransmissionAsync(activity, func, messages, cancellationToken);
        }
    }
}
