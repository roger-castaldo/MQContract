using MQContract.CQRS.Attributes;
using MQContract.CQRS.Interfaces.Query;

namespace CQRSTesting.Messages;

[Query("BasicQuery")]
public record BasicQuery(string Name) : IQuery
{
}
