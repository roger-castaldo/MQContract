using MQContract.CQRS.Interfaces;
using MQContract.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.CQRS.Extensions
{
    public static class IContractConnectionExtension
    {
        public static ICQRSConnection CreateCQRSConnection(this IContractConnection contractConnection, string? cancelationTokenChannel = null)
            => new CqrsConnection(contractConnection, cancelationTokenChannel);
    }
}
