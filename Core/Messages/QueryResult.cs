using MQContract.Interfaces;

namespace MQContract.Messages
{
    internal record QueryResult<T>(T? Result, IMessageHeader Header, string ID, bool IsError, string? Error)
        : IQueryResult<T> where T : class;
}
