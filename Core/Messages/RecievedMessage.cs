using MQContract.Interfaces;

namespace MQContract.Messages
{
    internal record RecievedMessage<T>(string ID,T Message,MessageHeader Headers,DateTime RecievedTimestamp,DateTime ProcessedTimestamp)
        : IRecievedMessage<T>
        where T : class
    {}
}
