using MQContract.Interfaces.Messages;

namespace MQContract.Messages;

/// <summary>
/// Used as the base of an Encoded Message which can be a ServiceMessage, ReceivedServiceMessage, or ServiceQueryResult. This is used to house the common properties of all of these message types and to avoid code duplication.
/// </summary>
public record EncodedMessage : IEncodedMessage, IDisposable
{
    private bool disposedValue;

    /// <summary>
    /// The message type id to transmit across
    /// </summary>
    public string MessageTypeID { get; }
    /// <summary>
    /// The header for the given message
    /// </summary>
    public MessageHeader Header { get; }
    /// <summary>
    /// The encoded message
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; set; }
    /// <summary>
    /// The unique ID of the message
    /// </summary>
    public string ID { get; }

    /// <summary>
    /// Default constructor for a Service Query Result
    /// </summary>
    /// <param name="id">The unique ID of the message</param>
    /// <param name="messageTypeID">An identifier that identifies the type of message encoded</param>
    /// <param name="header">The headers to transmit with the message</param>
    /// <param name="data">The content of the message</param>
    protected EncodedMessage(string id, MessageHeader header, string messageTypeID, ReadOnlyMemory<byte> data)
    {
        ID = id;
        MessageTypeID = messageTypeID;
        Header = header;
        Data = data;
    }

    void IDisposable.Dispose()
    {
        if (!disposedValue)
        {
            ((IDisposable)Header).Dispose();
            disposedValue=true;
        }
        GC.SuppressFinalize(this);
    }
}
