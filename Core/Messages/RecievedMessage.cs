﻿using MQContract.Interfaces;

namespace MQContract.Messages
{
    internal record RecievedMessage<T>(string ID,T Message,IMessageHeader Headers,DateTime RecievedTimestamp,DateTime ProcessedTimestamp)
        : IMessage<T>
        where T : class
    {}
}