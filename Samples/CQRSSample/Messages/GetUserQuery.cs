using MQContract.CQRS.Attributes;
using MQContract.CQRS.Interfaces.Query;

namespace Messages;

[Query(channel: "UserQueries", typeName: "GetUser", typeVersion: "1.0.0", responseChannel: "UserResponses")]
public record GetUserQuery(string UserId) : IQuery;