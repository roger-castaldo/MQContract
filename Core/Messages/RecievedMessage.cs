using MQContract.Interfaces;

namespace MQContract.Messages
{
    internal record ReceivedMessage<T>(string ID,T Message,MessageHeader Headers,DateTime ReceivedTimestamp,DateTime ProcessedTimestamp)
        : IReceivedMessage<T>
        where T : class
    {}
}
