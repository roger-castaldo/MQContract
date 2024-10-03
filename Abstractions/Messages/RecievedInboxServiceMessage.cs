using System.Diagnostics.CodeAnalysis;

namespace MQContract.Messages
{
    /// <summary>
    /// A Received Service Message that gets passed back up into the Contract Connection when a message is received from the underlying service connection
    /// </summary>
    /// <param name="ID">The unique ID of the message</param>
    /// <param name="MessageTypeID">The message type id which is used for decoding to a class</param>
    /// <param name="Channel">The channel the message was received on</param>
    /// <param name="Header">The message headers that came through</param>
    /// <param name="CorrelationID">The query message correlation id supplied by the query call to tie to the response</param>
    /// <param name="Data">The binary content of the message that should be the encoded class</param>
    /// <param name="Acknowledge">The acknowledgement callback to be called when the message is received if the underlying service requires it</param>
    [ExcludeFromCodeCoverage(Justification ="This is a record class and has nothing to test")]
    public record ReceivedInboxServiceMessage(string ID, string MessageTypeID, string Channel, MessageHeader Header,Guid CorrelationID, ReadOnlyMemory<byte> Data,Func<ValueTask>? Acknowledge=null)
        : ReceivedServiceMessage(ID,MessageTypeID,Channel,Header,Data,Acknowledge)
    {}
}
