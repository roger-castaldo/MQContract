namespace MQContract.InMemory
{
    public class TransmissionResultException : Exception
    {
        internal TransmissionResultException() 
         : base("Unable to transmit"){ }
    }
}
