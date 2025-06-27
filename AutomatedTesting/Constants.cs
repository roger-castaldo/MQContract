using AutomatedTesting.Messages;
using MQContract.Attributes;
using System.Reflection;

namespace AutomatedTesting
{
    internal static class Constants
    {
        public const string BasicMessageType = "BasicMessage-0.0.0.0";
        public const string CustomEncoderMessageType = "CustomEncoderMessage-0.0.0.0";
        public const string CustomEncryptorMessageType = "CustomEncryptorMessage-0.0.0.0";
        public const string CustomEncoderWithInjectionMessageType = "CustomEncoderWithInjectionMessage-0.0.0.0";
        public const string CustomEncryptorWithInjectionMessageType = "CustomEncryptorWithInjectionMessage-0.0.0.0";
        public const string NoChannelMessageType = "NoChannelMessage-0.0.0.0";
        public const string BasicQueryMessageType = "BasicQueryMessage-0.0.0.0";
        public const string TimeoutMessageType = "TimeoutMessage-0.0.0.0";
        public readonly static string NamedAndVersionedMessageType = $"{typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageNameAttribute>(false)?.Value}-{typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageVersionAttribute>(false)?.Version}";
    }
}
