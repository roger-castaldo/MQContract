using MQContract.Attributes;

namespace MQContract.CQRS.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class QueryAttribute(string channel, string? typeName = null, string? typeVersion = null,
        string? responseChannel = null, int responseTimeoutMilliseconds = 60*1000) : 
        QueryMessageAttribute(channel,typeName,typeVersion,responseChannel,responseTimeoutMilliseconds)
    {
    }
}
