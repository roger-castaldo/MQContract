using MQContract.Attributes;

namespace MQContract.CQRS.Attributes
{
    /// <summary>
    /// Use this attribute to specify the Channel, TypeName and or TypeVersion of the 
    /// Command being defined
    /// </summary>
    /// <example>
    /// <code>
    /// [Command(channel: "UserCommands", typeName: "CreateUser", typeVersion: "1.0.0")]
    /// public record CreateUserCommand(string UserId, string UserName, string Email) : ICommand;
    /// </code>
    /// </example>
    /// <param name="channel">The channel to be used</param>
    /// <param name="typeName">The command type to use</param>
    /// <param name="typeVersion">The command type version to use</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CommandAttribute(string channel, string? typeName = null, string? typeVersion = null)
        : MessageAttribute(channel, typeName, typeVersion)
    {
    }
}
