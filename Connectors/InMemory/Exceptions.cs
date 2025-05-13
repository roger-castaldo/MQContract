namespace MQContract.InMemory
{
    /// <summary>
    /// Thrown when a message transmission has failed within the In Memory system
    /// </summary>
    public class TransmissionResultException : Exception
    {
        internal TransmissionResultException() 
         : base("Unable to transmit"){ }
    }
}
