using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.ServiceAbstractions.Messages
{
    /// <summary>
    /// Houses the headers that were supplied by the respective service message
    /// </summary>
    public interface IMessageHeader
    {
        /// <summary>
        /// Called to get a value for the given key if it exists
        /// </summary>
        /// <param name="tagKey">The key name</param>
        /// <returns>null if the key is not found, otherwise it returns the key value</returns>
        string? this[string tagKey] { get; }
        /// <summary>
        /// The keys available in the header
        /// </summary>
        IEnumerable<string> Keys { get; }
    }
}
