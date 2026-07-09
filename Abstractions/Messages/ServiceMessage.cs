namespace MQContract.Messages;

/// <summary>
/// Houses a service level message that would be supplied to the underlying Service Connection for transmission purposes
/// </summary>
public record ServiceMessage : EncodedMessage
{
    /// <summary>
    /// The channel to transmit the message on.
    /// </summary>
    public string Channel { get; set; }
    
    /// <summary>
    /// Default constructor for a service message
    /// </summary>
    /// <param name="id">The unique ID of the message</param>
    /// <param name="messageTypeID">An identifier that identifies the type of message encoded</param>
    /// <param name="channel">The channel to transmit the message on</param>
    /// <param name="header">The headers to transmit with the message</param>
    /// <param name="data">The content of the message</param>
    public ServiceMessage(string id, string messageTypeID, string channel, MessageHeader header, ReadOnlyMemory<byte> data)
        : base(id, header, messageTypeID, data)
    {
        Channel = channel;
    }
}
