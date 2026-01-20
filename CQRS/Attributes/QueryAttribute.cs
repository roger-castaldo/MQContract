using MQContract.Attributes;

namespace MQContract.CQRS.Attributes
{
    /// <summary>
    /// Use this attribute to specify the Channel, TypeName, TypeVersion, Response Channel and or Response timeout 
    /// for the Query being defined
    /// </summary>
    /// <example>
    /// <code>
    /// [Query(channel: "UserQueries", typeName: "GetUser", typeVersion: "1.0.0", responseChannel: "UserResponses")]
    /// public record GetUserQuery(string UserId) : IQuery;
    /// </code>
    /// </example>
    /// <param name="channel">The channel to be used</param>
    /// <param name="typeName">The query type to use</param>
    /// <param name="typeVersion">The query type version to use</param>
    /// <param name="responseChannel">The responce channel to be used when an underlying service connection does not support QueryResponse or Inbox</param>
    /// <param name="responseTimeoutMilliseconds">The query response timeout to default to</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class QueryAttribute(string channel, string? typeName = null, string? typeVersion = null,
        string? responseChannel = null, int responseTimeoutMilliseconds = 60*1000) :
        QueryMessageAttribute(channel, typeName, typeVersion, responseChannel, responseTimeoutMilliseconds)
    {
    }
}
