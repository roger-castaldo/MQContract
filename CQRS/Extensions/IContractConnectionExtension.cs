using MQContract.CQRS.Interfaces;
using MQContract.Interfaces;

namespace MQContract.CQRS.Extensions
{
    /// <summary>
    /// Houses extension methods for the CQRS connections linked to a Contract Connection
    /// </summary>
    public static class IContractConnectionExtension
    {
        /// <summary>
        /// Creates a CQRS connection instance linked to the given contract connection
        /// WARNING:  THe Contract Connection cannot be a MultiService style connection, it only supports the single instance or mapped.
        /// </summary>
        /// <param name="contractConnection">The contract connection it will be linked to.</param>
        /// <param name="cancelationTokenChannel">The channel to use for distributing cancelling token Cancel calls</param>
        /// <returns></returns>
        public static ICQRSConnection CreateCQRSConnection(this IContractConnection contractConnection, string? cancelationTokenChannel = null)
            => new CqrsConnection(contractConnection, cancelationTokenChannel);
    }
}
