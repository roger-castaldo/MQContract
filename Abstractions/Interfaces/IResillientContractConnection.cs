namespace MQContract.Interfaces
{
    public interface IResillientContractConnection : IBaseContractConnection
    {
        void RegisterResiliencePolicy(
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy = null
        );
        void RegisterResiliencePolicy<T>(
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy = null
        );
        void RegisterResiliencePolicy(
            Type messageType,
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy = null
        );
        void RegisterResiliencePolicy(
            string messageChannel,
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy = null
        );
    }
}
