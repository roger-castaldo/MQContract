using MQContract.Messages;

namespace MQContract.Interfaces.Consumers
{
    /// <summary>
    /// Used to define a consumer that will filter out messages of a given message type
    /// </summary>
    /// <typeparam name="TMessage">The type of message the filter understands</typeparam>
    public interface IMessageFilteredConsumer<TMessage> : IBaseConsumer
    {
        /// <summary>
        /// Provides the filter callback that will be supplied the message headers and current message 
        /// and expects back a filter instruction
        /// </summary>
        Func<TMessage, MessageHeader, ValueTask<MessageFilterResult>> Filter { get; }
    }
}
