using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Attributes
{
    /// <summary>
    /// Use this attribute to specify the GroupName that this Consumer will use when registering 
    /// to supply to the underlying subscription.  This can be overriden in the registration call by passing 
    /// a value for the group input.
    /// </summary>
    /// <param name="name">The name of the Group to identify this Consumer and it's underlying subscription</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ConsumerGroupAttribute(string name) : Attribute
    {
        /// <summary>
        /// The name of the Group specified for this Consumer toi be part of
        /// </summary>
        public string Name => name;
    }
}
