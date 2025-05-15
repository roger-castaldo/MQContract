using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Interfaces
{
    /// <summary>
    /// A Mappable Contract Connection which supports mapping one or more Service Connections to a given message type, channel and or headers
    /// This also defines the extended resillience functionality to allow for a resillience policy to be set at the connection level
    /// </summary>
    /// <typeparam name="CC">The underlying type that is being represented here which must be IBaseContractConnection, CC is used for method chaining.</typeparam>
    public interface IMappableContractConnection<CC> : IConsumerContractConnection
        where CC : IBaseContractConnection
    {
        /// <summary>
        /// Register a service connection using a callback for mapping
        /// </summary>
        /// <param name="checkCallback">The callback to check if this connection can be used with the given parameters</param>
        /// <param name="serviceConnectionName">The name of the service connection, not necessarily unique, but can be used for logging and other things</param>
        /// <param name="messageServiceConnection">The service connection to use when the checkCallback returns true</param>
        /// <returns></returns>
        CC RegisterServiceConnection(Func<(string channel, Type messageType, MessageHeader messageHeader), bool> checkCallback, string serviceConnectionName, IMessageServiceConnection messageServiceConnection);
        /// <summary>
        /// Register a service connection for a given channel
        /// </summary>
        /// <param name="channel">The channel that the service connection should be used for</param>
        /// <param name="serviceConnectionName">The name of the service connection, not necessarily unique, but can be used for logging and other things</param>
        /// <param name="messageServiceConnection">The service connection to use when the channel is used</param>
        /// <returns></returns>
        CC RegisterServiceConnection(string channel, string serviceConnectionName, IMessageServiceConnection messageServiceConnection);
        /// <summary>
        /// Register a service connection for a given message type
        /// </summary>
        /// <param name="messageType">The type of message that the service connection should be used for</param>
        /// <param name="serviceConnectionName">The name of the service connection, not necessarily unique, but can be used for logging and other things</param>
        /// <param name="messageServiceConnection">The service connection to use when the messageType is used</param>
        /// <returns></returns>
        CC RegisterServiceConnection(Type messageType, string serviceConnectionName, IMessageServiceConnection messageServiceConnection);
        /// <summary>
        /// Register a service connection for a given message type
        /// </summary>
        /// <typeparam name="T">The type of message that the service connection should be used for</typeparam>
        /// <param name="serviceConnectionName">The name of the service connection, not necessarily unique, but can be used for logging and other things</param>
        /// <param name="messageServiceConnection">The service connection to use when the message is of type T</param>
        /// <returns></returns>
        CC RegisterServiceConnection<T>(string serviceConnectionName, IMessageServiceConnection messageServiceConnection);
        /// <summary>
        /// Register a service connection to be used when the messageHeader contains the messageHeaderKey and it's value is messageHeaderValue
        /// </summary>
        /// <param name="messageHeaderKey">The key value for the message header</param>
        /// <param name="messageHeaderValue">The value for the message header</param>
        /// <param name="serviceConnectionName">The name of the service connection, not necessarily unique, but can be used for logging and other things</param>
        /// <param name="messageServiceConnection">The service connection to use when the messageHeader has the key and the value matches</param>
        /// <returns></returns>
        CC RegisterServiceConnection(string messageHeaderKey, string messageHeaderValue, string serviceConnectionName, IMessageServiceConnection messageServiceConnection);
        /// <summary>
        /// Register a default resiliency policy that will apply to any message transmissions that do not have a specific policy
        /// </summary>
        /// <param name="serviceConnectionName">The name of the service connection, not necessarily unique, but can be used for logging and other things</param>
        /// <param name="retryPolicy">The settings to use for retries if desired</param>
        /// <param name="circuitBreakPolicy">The settings to use for circuit breaking if desired</param>
        CC RegisterResiliencePolicy(
            string serviceConnectionName,
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy = null
        );
        /// <summary>
        /// Register a resiliency policy that will apply to any message transmission of message type T
        /// </summary>
        /// <typeparam name="T">The type of message to associate this policy to</typeparam>
        /// <param name="serviceConnectionName">The name of the service connection, not necessarily unique, but can be used for logging and other things</param>
        /// <param name="retryPolicy">The settings to use for retries if desired</param>
        /// <param name="circuitBreakPolicy">The settings to use for circuit breaking if desired</param>
        CC RegisterResiliencePolicy<T>(
            string serviceConnectionName,
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy = null
        );
        /// <summary>
        /// Register a resiliency policy that will apply to any message transmission of the given messageType
        /// </summary>
        /// <param name="serviceConnectionName">The name of the service connection, not necessarily unique, but can be used for logging and other things</param>
        /// <param name="messageType">The type of message to associate this policy to</param>
        /// <param name="retryPolicy">The settings to use for retries if desired</param>
        /// <param name="circuitBreakPolicy">The settings to use for circuit breaking if desired</param>
        CC RegisterResiliencePolicy(
            string serviceConnectionName,
            Type messageType,
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy = null
        );
        /// <summary>
        /// Register a resiliency policy that will apply to any message transmission of a message on the given channel
        /// </summary>
        /// <param name="serviceConnectionName">The name of the service connection, not necessarily unique, but can be used for logging and other things</param>
        /// <param name="messageChannel">The message channel to apply this policy to</param>
        /// <param name="retryPolicy">The settings to use for retries if desired</param>
        /// <param name="circuitBreakPolicy">The settings to use for circuit breaking if desired</param>
        CC RegisterResiliencePolicy(
            string serviceConnectionName,
            string messageChannel,
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null,
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakPolicy = null
        );
    }
}
