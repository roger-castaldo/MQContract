using MQContract.Interfaces.Encoding;

namespace MQContract.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class UseMqContractAttribute : Attribute
    {
        public Type ContractType { get; }
        public Type? EncoderType { get; }
        public Type[]? Converters { get; }
        public Type? MessageEncryptor { get; }
        public UseMqContractAttribute(Type contractType, Type? encoderType = null, Type[]? converters = null, Type? messageEncryptor = null)
        {
            if (encoderType!=null && !encoderType.GetInterfaces().Any(t => Equals(t, typeof(IMessageTypeEncoder<>).MakeGenericType(contractType))))
                throw new Exception($"Cannot link an encoder type that does not implement the interface IMessageTypeEncoder<{contractType.Name}>");
            if (messageEncryptor!=null && !messageEncryptor.GetInterfaces().Any(t=>Equals(t,typeof(IMessageTypeEncoder<>).MakeGenericType(contractType))))
                throw new Exception($"Cannot link an encryptor type that does not implement the interface IMessageTypeEncoder<{contractType.Name}>");
            ContractType = contractType;
            EncoderType = encoderType;
            Converters = converters;
            MessageEncryptor=messageEncryptor;
        }
    }
}
