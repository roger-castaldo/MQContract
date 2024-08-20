﻿using MQContract.Interfaces.Conversion;
using MQContract.Messages;

namespace MQContract.Interfaces.Factories
{
    internal interface IMessageFactory<T> : IMessageTypeFactory, IConversionPath<T> where T : class
    {
        ValueTask<ServiceMessage> ConvertMessageAsync(T message, string? channel, MessageHeader? messageHeader,Func<string,Task<string>>? mapChannel=null);
    }
}
