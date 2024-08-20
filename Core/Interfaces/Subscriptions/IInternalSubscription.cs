using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Interfaces.Subscriptions
{
    internal interface IInternalSubscription : ISubscription
    {
        Guid ID { get; }

        Task EndAsync(bool remove);
    }
}
