using MQContract.Messages;

namespace MQContract.Interfaces.Consumers
{
    /// <summary>
    /// Represents a QueryResponse Message Consumer to be registered to the ContractConnection
    /// </summary>
    /// <typeparam name="TQuery">The type of Message that is received and will be consumed</typeparam>
    /// <typeparam name="TQueryResponse">The type of Message that is returned as a response</typeparam>
    public interface IQueryResponseConsumer<TQuery, TQueryResponse> : IBaseConsumer
    {
        /// <summary>
        /// Called when a message is received from the underlying subscript that is using this Consumer
        /// </summary>
        /// <param name="message">The message that was received</param>
        QueryResponseMessage<TQueryResponse> MessageReceived(IReceivedMessage<TQuery> message);
    }
}
