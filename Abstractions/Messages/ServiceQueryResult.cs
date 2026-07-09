namespace MQContract.Messages;

/// <summary>
/// Houses a result from a query call from the Service Connection Level
/// </summary>
public record ServiceQueryResult : EncodedMessage
{
    /// <summary>
    /// Default constructor for a Service Query Result
    /// </summary>
    /// <param name="id">The unique ID of the message</param>
    /// <param name="messageTypeID">An identifier that identifies the type of message encoded</param>
    /// <param name="header">The headers to transmit with the message</param>
    /// <param name="data">The content of the message</param>
    public ServiceQueryResult(string id, MessageHeader header, string messageTypeID, ReadOnlyMemory<byte> data)
        : base(id, header, messageTypeID, data) { }
}
