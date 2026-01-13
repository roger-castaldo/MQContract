namespace MQContract.Attributes
{
    /// <summary>
    /// Used as part of the Message context for code generation to specify auto scanning settings.  Must be attached to an implementation of MQContractMessageContext.
    /// </summary>
    /// <param name="locateEncoders">Set to true if you want to automatically scan for encoders for a message when none are specified</param>
    /// <param name="locateConverters">Set to true if you want to automatically scan for converters for a message when none are specified</param>
    /// <param name="locateEncryptors">Set to true if you want to automatically scan for encryptors for a message when none are specified</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MQContractMessageContextAttribute(bool locateEncoders = false, bool locateConverters = false, bool locateEncryptors = false) : Attribute
    {
        /// <summary>
        /// Indicates if Encoders are to be located when none are specified
        /// </summary>
        public bool LocateEncoders => locateEncoders;
        /// <summary>
        /// Indicates if Converters are to be located when none are specified
        /// </summary>
        public bool LocateConverters => locateConverters;
        /// <summary>
        /// Indicates if Encryptors are to be located when none are specified
        /// </summary>
        public bool LocateEncryptors => locateEncryptors;

    }
}
