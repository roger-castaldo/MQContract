using MQContract.Messages;

namespace MQContract.InMemory
{
    internal record InternalServiceMessage(string ID, string MessageTypeID, string Channel, MessageHeader Header, ReadOnlyMemory<byte> Data,Guid? CorrelationID=null,string? ReplyChannel=null)
        : ServiceMessage(ID,MessageTypeID,Channel,Header, Data)
    { }
}
