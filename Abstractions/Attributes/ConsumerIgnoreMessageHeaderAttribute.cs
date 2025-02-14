using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Attributes
{
    /// <summary>
    /// Use this attribute to specify the if this consumer should ignore the message header as part of it's
    /// underlying subscription.
    /// </summary>
    /// <param name="ignoreHeader">If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ConsumerIgnoreMessageHeaderAttribute(bool ignoreHeader) : Attribute
    {
        /// <summary>
        /// Indicates whether the underlying subscript should ignore the message header
        /// </summary>
        public bool IgnoreHeader => ignoreHeader;
    }
}
