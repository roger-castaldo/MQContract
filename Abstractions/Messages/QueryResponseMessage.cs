namespace MQContract.Messages;

/// <summary>
/// Houses the Query Response Message to be sent back from a query call
/// </summary>
/// <typeparam name="TQueryResponse">The type of message contained in the response</typeparam>
/// <param name="Message">The message to respond back with</param>
/// <param name="Headers">The headers to attach to the response</param>
public record QueryResponseMessage<TQueryResponse>(TQueryResponse Message, IEnumerable<KeyValuePair<string, string?>>? Headers = null);
