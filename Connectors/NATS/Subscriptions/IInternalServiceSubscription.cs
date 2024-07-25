using MQContract.Interfaces.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.NATS.Subscriptions
{
    internal interface IInternalServiceSubscription : IServiceSubscription
    {
        void Run();
    }
}
