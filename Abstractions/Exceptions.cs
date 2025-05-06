namespace MQContract
{
    public sealed class TransmissionException(Exception underlyingError, bool isFatal = false) 
        : Exception("Transmission Exception occured",underlyingError)
    {
        public bool IsFatal => isFatal;
    }

    public enum ResillianceTypes
    {
        Retry,
        CircuitBreak
    };

    public sealed class ResillianceException(ResillianceTypes type, Exception error) 
        : Exception("The action failed through the resilliance", error)
    {
        public ResillianceTypes Type => type;
    }
}
