using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Interfaces
{
    /// <summary>
    /// A Mappable Contract Connection which supports mapping one or more Service Connections to a given message type, channel and or headers
    /// </summary>
    /// <typeparam name="CC">The underlying type that is being represented here which must be IBaseContractConnection, CC is used for method chaining.</typeparam>
    public interface IMappableContractConnection<CC> : IBaseContractConnection
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
        CC RegisterServiceConnection<T>(string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            where T : class;
        /// <summary>
        /// Register a service connection to be used when the messageHeader contains the messageHeaderKey and it's value is messageHeaderValue
        /// </summary>
        /// <param name="messageHeaderKey">The key value for the message header</param>
        /// <param name="messageHeaderValue">The value for the message header</param>
        /// <param name="serviceConnectionName">The name of the service connection, not necessarily unique, but can be used for logging and other things</param>
        /// <param name="messageServiceConnection">The service connection to use when the messageHeader has the key and the value matches</param>
        /// <returns></returns>
        CC RegisterServiceConnection(string messageHeaderKey, string messageHeaderValue, string serviceConnectionName, IMessageServiceConnection messageServiceConnection);
    }
}
