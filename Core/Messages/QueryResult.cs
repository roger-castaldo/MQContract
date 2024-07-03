using MQContract.Interfaces;

namespace MQContract.Messages
{
    internal record QueryResult<T>(T? Result, IMessageHeader Header, string MessageID, bool IsError, string? Error)
        : IQueryResult<T> where T : class;
}
