using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.InMemory
{
    public class TransmissionResultException : Exception
    {
        internal TransmissionResultException() 
         : base("Unable to transmit"){ }
    }
}
