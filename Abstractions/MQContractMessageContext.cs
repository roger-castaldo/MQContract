using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Messages;
using MQContract.Interfaces.Middleware;
using MQContract.Messages;

namespace MQContract
{
    /// <summary>
    /// Used to attache messages for the code generator to build up code for to improve performance and potentially handle AOT.  The implementation of this class must be made as 
    /// partial and none of the virtual calls need to be implemented as the code generator will handle that.
    /// </summary>
    /// <example>
    /// using MQContract;
    /// 
    /// namespace Messages;
    /// 
    /// [UseMqContractAttribute(typeof(Announcement))]
    /// public partial class MyMessageContext : MQContractMessageContext { }
    /// </example>
    public abstract class MQContractMessageContext
    {
        /// <summary>
        /// Used to house Message Type Definitions that are built both through attributes and other class aspects
        /// </summary>
        /// <param name="Channel">The channel that the message is to use by default</param>
        /// <param name="TypeName">The Type Name used within the Message ID</param>
        /// <param name="TypeVersion">The Version used within the Message ID</param>
        /// <param name="ResponseChannel">The response channel to use if specified</param>
        /// <param name="ResponseTimeout">The response timeout to use if specified</param>
        /// <param name="ResponseType">The response type to use if specified</param>
        public readonly record struct MessageTypeDefinition(string? Channel, string TypeName, Version TypeVersion, string? ResponseChannel, TimeSpan? ResponseTimeout, Type? ResponseType);

        /// <summary>
        /// Called to attempt to get the MessageEncoder specified for a given message.
        /// </summary>
        /// <remarks>This will be implemented by the code generator</remarks>
        /// <typeparam name="TMessage">The type of message to locate the encoder for</typeparam>
        /// <param name="globalMessageEncoder">The global message encoder specified for this contract if any</param>
        /// <param name="serviceProvider">An instance of the ServiceProvider used for DI if available</param>
        /// <returns>null or an instance of an encoder to use</returns>
        public virtual object? TryGetMessageEncoder<TMessage>(IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
            => null;

        /// <summary>
        /// Called to attempt to get the DecodingCallback for a given message
        /// </summary>
        /// <remarks>This will be implemented by the code generator</remarks>
        /// <param name="messageID">The message type id of the service message</param>
        /// <param name="globalMessageEncoder">The global message encoder specified for this contract if any</param>
        /// <param name="serviceProvider">An instance of the ServiceProvider used for DI if available</param>
        /// <returns>null or an instance of a decode callback</returns>
        public virtual Func<IEncodedMessage, ValueTask<object?>>? TryGetDecodingCallback(string messageID, IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
            => null;

        /// <summary>
        /// Called to obtain the Message Definition housed if available
        /// </summary>
        /// <remarks>This will be implemented by the code generator</remarks>
        /// <param name="messageType">The type of message to locate</param>
        /// <returns>null or the definition for the given message type</returns>
        public virtual MessageTypeDefinition? TryGetMessageType(Type messageType)
            => null;

        /// <summary>
        /// Called to obtain a Message Converter to convert from the given message type id to the destination
        /// </summary>
        /// <remarks>This will be implemented by the code generator</remarks>
        /// <typeparam name="TMessage">The type of message to convert to</typeparam>
        /// <param name="messageID">The message type id of the service message</param>
        /// <param name="messageDecode">The service message decode call back obtained in another call to be able to decode the service message</param>
        /// <param name="serviceProvider">An instance of the ServiceProvider used for DI if available</param>
        /// <returns>null or an instance of a conversion callback</returns>
        public virtual Func<IEncodedMessage, ValueTask<object?>>? TryGetMessageConverter<TMessage>(string messageID, Func<IEncodedMessage, ValueTask<object?>> messageDecode, IServiceProvider? serviceProvider)
            => null;

        /// <summary>
        /// Called to determine if this context handles this particular message type
        /// </summary>
        /// <typeparam name="TMessage">The type of message to check for</typeparam>
        /// <returns>true if this context instance defines this message type</returns>
        public bool IsMessageCodeGenerated<TMessage>()
            => IsMessageCodeGenerated(typeof(TMessage));

        /// <summary>
        /// Called to determine if this context handles this particular message type
        /// </summary>
        /// <remarks>This will be implemented by the code generator</remarks>
        /// <param name="messageType">The type of message to check for</param>
        /// <returns>true if this context instance defines this message type</returns>
        public virtual bool IsMessageCodeGenerated(Type messageType)
            => false;

        /// <summary>
        /// Called to obtain a Message Encryptor for a given message type
        /// </summary>
        /// <remarks>This will be implemented by the code generator</remarks>
        /// <param name="messageType">The type of message to check for</param>
        /// <param name="globalEncryptor">The instance of the global encryptor for the connection if supplied</param>
        /// <param name="serviceProvider">An instance of the ServiceProvider used for DI if available</param>
        /// <returns>null or an instance of an encryptor for the given message</returns>
        public virtual IMessageEncryptor? TryGetMessageEncryptor(Type messageType, IMessageEncryptor? globalEncryptor, IServiceProvider? serviceProvider)
            => null;

        /// <summary>
        /// Called to attempt to execute a Query call with an unkown return type
        /// </summary>
        /// <remarks>This will be implemented by the code generator</remarks>
        /// <typeparam name="TQuery">The type of message to query with</typeparam>
        /// <param name="contractConnection">An instance of the connection to invoke it against</param>
        /// <param name="message">The query message</param>
        /// <param name="timeout">The query timeout</param>
        /// <param name="channel">The channel to use</param>
        /// <param name="responseChannel">The response channel to use</param>
        /// <param name="messageHeader">The message header to use</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>null or the Query attempt against the given connection</returns>
        public virtual ValueTask<QueryResult<object>>? TryExecuteQuery<TQuery>(IContractConnection contractConnection, object message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
            => null;

        /// <summary>
        /// Called to attempt to execute a Query call with an unkown return type
        /// </summary>
        /// <remarks>This will be implemented by the code generator</remarks>
        /// <typeparam name="TQuery">The type of message to query with</typeparam>
        /// <param name="contractConnection">An instance of the connection to invoke it against</param>
        /// <param name="message">The query message</param>
        /// <param name="timeout">The query timeout</param>
        /// <param name="channel">The channel to use</param>
        /// <param name="responseChannel">The response channel to use</param>
        /// <param name="messageHeader">The message header to use</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>null or the Query attempt against the given connection</returns>
        public virtual ValueTask<IEnumerable<QueryResult<object>>>? TryExecuteQuery<TQuery>(IMultiServiceContractConnection contractConnection, object message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
            => null;

        /// <summary>
        /// Lists the messages that are defined within this context
        /// </summary>
        public virtual IEnumerable<MessageContextDefintion> DefinedMessages 
            => [];
    }
}
