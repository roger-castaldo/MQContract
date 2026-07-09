namespace MQContract.Messages;

/// <summary>
/// Houses a set of message filtering calls for a given type.  This particular record can be passed in to pubsub consumers/subscriptions
/// to implement some pre-callback message filtering when receiving messages
/// </summary>
/// <typeparam name="TMessage">The type of message that the subscription and filter represents</typeparam>
/// <param name="HeaderFilter">A callback filter used to filter a message by headers.  This call is made prior to attempting to convert the message into the appropriate type.</param>
/// <param name="MessageFilter">A callback filter used to filter a message by the message and or headers.  This call is made after the attempt to convert the message into the approriate type.</param>
public record MessageFilters<TMessage>(
    Func<MessageHeader, ValueTask<MessageFilterResult>>? HeaderFilter = null,
    Func<TMessage, MessageHeader, ValueTask<MessageFilterResult>>? MessageFilter = null
);
