using MQContract.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.KubeMQ.Interfaces
{
    public interface IKubeMQPingResult
    {
        /// <summary>
        /// The Server Start Time of the host that was pinged
        /// </summary>
        DateTime ServerStartTime { get; }
        /// <summary>
        /// The Server Up Time of the host that was pinged
        /// </summary>
        TimeSpan ServerUpTime { get; }
    }
}
