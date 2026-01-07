namespace MQContract.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MQContractMessageContextAttribute : Attribute
    {
        public bool LocateEncoders { get; }
        public bool LocateConverters { get; }
        public bool LocateEncryptors { get; }

        public MQContractMessageContextAttribute(bool locateEncoders = false, bool locateConverters = false, bool locateEncryptors = false)        
        {
            LocateEncoders=locateEncoders;
            LocateConverters=locateConverters;
            LocateEncryptors=locateEncryptors;
        }

    }
}
