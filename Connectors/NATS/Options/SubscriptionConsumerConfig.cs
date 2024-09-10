using NATS.Client.JetStream.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.NATS.Options
{
    internal record SubscriptionConsumerConfig(string Channel,ConsumerConfig Configuration)
    {
    }
}
