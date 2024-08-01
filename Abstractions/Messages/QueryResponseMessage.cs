namespace MQContract.Messages
{
    /// <summary>
    /// Houses the Query Response Message to be sent back from a query call
    /// </summary>
    /// <typeparam name="T">The type of message contained in the response</typeparam>
    /// <param name="Message">The message to respond back with</param>
    /// <param name="Headers">The headers to attach to the response</param>
    public record QueryResponseMessage<T>(T Message,Dictionary<string,string?>? Headers = null);
}
