using MQContract.Interfaces;
using System.Diagnostics;

namespace MQContract.Messages
{
    internal record ReceivedMessage<T>(string ID,T Message,MessageHeader Headers,DateTime ReceivedTimestamp,DateTime ProcessedTimestamp,Activity? Activity)
        : IReceivedMessage<T>
    {}
}
