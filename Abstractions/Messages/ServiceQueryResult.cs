using MQContract.Interfaces.Messages;

namespace MQContract.Messages
{
    /// <summary>
    /// Houses a result from a query call from the Service Connection Level
    /// </summary>
    /// <param name="ID">The ID of the message</param>
    /// <param name="Header">The headers transmitted</param>
    /// <param name="MessageTypeID">The type of message encoded</param>
    /// <param name="Data">The encoded data of the message</param>
    public record ServiceQueryResult(string ID, MessageHeader Header,string MessageTypeID,ReadOnlyMemory<byte> Data)
        : IEncodedMessage
    {
    }
}
