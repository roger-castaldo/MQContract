using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.NATS
{
    /// <summary>
    /// Thrown when an error occurs attempting to connect to the NATS server.  
    /// Specifically this will be thrown when the Ping that is executed on each initial connection fails.
    /// </summary>
    public class UnableToConnectException : Exception
    {
        internal UnableToConnectException()
            : base("Unable to establish connection to the NATS host") { }
    }

    internal class QueryAsyncReponseException : Exception
    {
        internal QueryAsyncReponseException(string error)
            : base(error) { }
    }
}
