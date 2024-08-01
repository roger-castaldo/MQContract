using MQContract.Interfaces.Messages;

namespace MQContract.Messages
{
    /// <summary>
    /// Houses a service level message that would be supplied to the underlying Service Connection for transmission purposes
    /// </summary>
    /// <param name="ID">The unique ID of the message</param>
    /// <param name="MessageTypeID">An identifier that identifies the type of message encoded</param>
    /// <param name="Channel">The channel to transmit the message on</param>
    /// <param name="Header">The headers to transmit with the message</param>
    /// <param name="Data">The content of the message</param>
    public record ServiceMessage(string ID,string MessageTypeID,string Channel,MessageHeader Header,ReadOnlyMemory<byte> Data)
        : IEncodedMessage
    { }
}
