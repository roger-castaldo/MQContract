namespace MQContract.Interfaces
{
    public interface IResillientContractConnection : IBaseContractConnection
    {
        void RegisterTransientPublishPolicy(
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy = null
        );
        void RegisterTransientPublishPolicy<T>(
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy = null
        );
        void RegisterTransientPublishPolicy(
            Type messageType,
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy = null
        );
        void RegisterTransientPublishPolicy(
            string messageChannel,
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy = null
        );
    }
}
