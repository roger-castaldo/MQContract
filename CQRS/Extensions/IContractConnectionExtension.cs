using MQContract.CQRS.Interfaces;
using MQContract.Interfaces;

namespace MQContract.CQRS.Extensions
{
    public static class IContractConnectionExtension
    {
        public static ICQRSConnection CreateCQRSConnection(this IContractConnection contractConnection, string? cancelationTokenChannel = null)
            => new CqrsConnection(contractConnection, cancelationTokenChannel);
    }
}
