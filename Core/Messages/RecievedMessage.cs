using MQContract.Interfaces;
using System.Diagnostics;

namespace MQContract.Messages;

internal record ReceivedMessage<TMessage>(string ID, TMessage Message, MessageHeader Headers, DateTime ReceivedTimestamp, DateTime ProcessedTimestamp, Activity? Activity)
    : IReceivedMessage<TMessage>
{ }
