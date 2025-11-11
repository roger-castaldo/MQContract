using MQContract.Attributes;

namespace MQContract.CQRS.Attributes
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =false,Inherited =true)]
    public class CommandAttribute(string channel, string? typeName = null, string? typeVersion = null) 
        : MessageAttribute(channel,typeName,typeVersion)
    {
    }
}
