using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Messages
{
    public record TransmissionResult(string ID,string? Error=null)
    {
        public bool IsError=>!string.IsNullOrWhiteSpace(Error);
    }
}
