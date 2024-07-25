using MQContract.Interfaces.Service;
using NATS.Client.JetStream.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.NATS.Options
{
    public record PublishSubscriberOptions(StreamConfig? StreamConfig=null,ConsumerConfig? ConsumerConfig=null) : IServiceChannelOptions
    {
    }
}
