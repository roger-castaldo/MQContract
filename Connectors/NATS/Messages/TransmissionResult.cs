using MQContract.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.NATS.Messages
{
    internal class TransmissionResult(string messageID,string? error=null) : ITransmissionResult
    {
        public string ID => messageID;

        public bool IsError => !string.IsNullOrWhiteSpace(Error);

        public string? Error => error;
    }
}
