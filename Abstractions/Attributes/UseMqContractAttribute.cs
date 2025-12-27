namespace MQContract.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class UseMqContractAttribute : Attribute
    {
        public Type ContractType { get; }
        public UseMqContractAttribute(Type contractType)
        {
            ContractType = contractType;
        }
    }
}
