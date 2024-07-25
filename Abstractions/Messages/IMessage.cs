using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Messages
{
    public interface IMessage
    {
        /// <summary>
        /// The ID of the message that was generated during transmission
        /// </summary>
        string ID { get; }
    }
}
