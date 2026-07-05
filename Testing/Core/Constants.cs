using CoreTesting.Messages;
using MQContract.Attributes;
using System.Reflection;

namespace CoreTesting
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
        public readonly static string NamedAndVersionedMessageType = $"{typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageAttribute>(false)?.TypeName}-{typeof(NamedAndVersionedMessage).GetCustomAttribute<MessageAttribute>(false)?.TypeVersion}";
        public const string HealthyDescription = "MQContract service connection available";
        public const string UnHealthyDescription = "MQContract service connection unavailable";
        public const string DegradedDescription = "1 or more service connection(s) are unavailable";
    }
}
