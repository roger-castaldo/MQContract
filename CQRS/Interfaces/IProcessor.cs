namespace MQContract.CQRS.Interfaces
{
    public interface IProcessor
    {
        void ErrorRecieved(Exception error);
    }
}
