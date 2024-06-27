using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract
{
    /// <summary>
    /// Thrown when an incoming data message causes a null object return from a converter
    /// </summary>
    public class MessageConversionException : Exception
    {
        internal MessageConversionException(Type messageType, Type converterType)
            : base($"The attempt to convert the incoming message resulted in a null object.[MessageType:{messageType.FullName},ConverterType:{converterType.FullName}]") { }
    }
}
