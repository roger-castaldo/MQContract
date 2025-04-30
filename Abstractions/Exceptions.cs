using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract
{
    public sealed class TransmissionException(Exception underlyingError, bool isFatal = false) 
        : Exception("Transmission Exception occured",underlyingError)
    {
        public bool IsFatal => isFatal;
    }
}
