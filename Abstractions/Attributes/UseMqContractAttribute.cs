using MQContract.Interfaces.Encoding;

namespace MQContract.Attributes
{
    /// <summary>
    /// Used to mark a message for a Message Context to be included in the code generation.  Must be attached to an implementation of MQContractMessageContext.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class UseMqContractAttribute : Attribute
    {
        /// <summary>
        /// The Message(Contract) type
        /// </summary>
        public Type ContractType { get; }
        /// <summary>
        /// The type of Encoder to use with it if specified
        /// </summary>
        public Type? EncoderType { get; }
        /// <summary>
        /// The type of Converters to use with it if specified
        /// </summary>
        public Type[]? Converters { get; }
        /// <summary>
        /// The type of Encryptor to use with it if specified
        /// </summary>
        public Type? MessageEncryptor { get; }
        /// <summary>
        /// Primary constructor
        /// </summary>
        /// <param name="contractType">The type of Message(Contract) to link</param>
        /// <param name="encoderType">The type of Encoder to use with it if specifically desired</param>
        /// <param name="converters">The type of Converters to use with it if specifically desired</param>
        /// <param name="messageEncryptor">The type of Encryptor to use with it if specifically desired</param>
        /// <exception cref="Exception"></exception>
        public UseMqContractAttribute(Type contractType, Type? encoderType = null, Type[]? converters = null, Type? messageEncryptor = null)
        {
            if (encoderType!=null && !encoderType.GetInterfaces().Any(t => Equals(t, typeof(IMessageTypeEncoder<>).MakeGenericType(contractType))))
                throw new InvalidEncoderException(contractType);
            if (messageEncryptor!=null && !messageEncryptor.GetInterfaces().Any(t=>Equals(t,typeof(IMessageTypeEncoder<>).MakeGenericType(contractType))))
                throw new InvalidEncryptorException(contractType);
            ContractType = contractType;
            EncoderType = encoderType;
            Converters = converters;
            MessageEncryptor=messageEncryptor;
        }
    }
}
