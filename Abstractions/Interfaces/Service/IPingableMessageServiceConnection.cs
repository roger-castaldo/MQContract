using MQContract.Messages;

namespace MQContract.Interfaces.Service
{
    /// <summary>
    /// Extends the base MessageServiceConnection Interface to support service pinging
    /// </summary>
    public interface IPingableMessageServiceConnection : IMessageServiceConnection
    {
        /// <summary>
        /// Implemented Ping call if avaialble for the underlying service
        /// </summary>
        /// <returns>A Ping Result</returns>
        ValueTask<PingResult> PingAsync();
    }
}
