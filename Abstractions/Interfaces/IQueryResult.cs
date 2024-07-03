using MQContract.Messages;

namespace MQContract.Interfaces
{
    public interface IQueryResult<out T> : ITransmissionResult where T : class
    {
        T? Result { get; }
        IMessageHeader Header { get; }
    }
}
