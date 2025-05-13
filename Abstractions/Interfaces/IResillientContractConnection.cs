namespace MQContract.Interfaces
{
    /// <summary>
    /// This interface represents the Resiliency extensions for the ContractConnection
    /// </summary>
    /// <typeparam name="CC">The underlying type that is being represented here which must be IBaseContractConnection, CC is used for method chaining.</typeparam>
    /// <remarks>All policies are applied against a response where the Error is set and the Error is not Fatal.
    /// Resilience policies are selected in the following order, whichever one matches first:
    /// Channel
    /// Type
    /// Default 
    /// </remarks>
    public interface IResillientContractConnection<CC> : IConsumerContractConnection
        where CC : IBaseContractConnection
    {
        /// <summary>
        /// Register a default resiliency policy that will apply to any message transmissions that do not have a specific policy
        /// </summary>
        /// <param name="retryPolicy">The settings to use for retries if desired</param>
        /// <param name="circuitBreakPolicy">The settings to use for circuit breaking if desired</param>
        CC RegisterResiliencePolicy(
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy = null
        );
        /// <summary>
        /// Register a resiliency policy that will apply to any message transmission of message type T
        /// </summary>
        /// <typeparam name="T">The type of message to associate this policy to</typeparam>
        /// <param name="retryPolicy">The settings to use for retries if desired</param>
        /// <param name="circuitBreakPolicy">The settings to use for circuit breaking if desired</param>
        CC RegisterResiliencePolicy<T>(
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy = null
        );
        /// <summary>
        /// Register a resiliency policy that will apply to any message transmission of the given messageType
        /// </summary>
        /// <param name="messageType">The type of message to associate this policy to</param>
        /// <param name="retryPolicy">The settings to use for retries if desired</param>
        /// <param name="circuitBreakPolicy">The settings to use for circuit breaking if desired</param>
        CC RegisterResiliencePolicy(
            Type messageType,
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy = null
        );
        /// <summary>
        /// Register a resiliency policy that will apply to any message transmission of a message on the given channel
        /// </summary>
        /// <param name="messageChannel">The message channel to apply this policy to</param>
        /// <param name="retryPolicy">The settings to use for retries if desired</param>
        /// <param name="circuitBreakPolicy">The settings to use for circuit breaking if desired</param>
        CC RegisterResiliencePolicy(
            string messageChannel,
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy = null
        );
    }
}
