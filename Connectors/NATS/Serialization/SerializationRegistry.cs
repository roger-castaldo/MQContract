using NATS.Client.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.NATS.Serialization
{
    internal class SerializationRegistry : INatsSerializerRegistry
    {
        public static readonly INatsSerializerRegistry Default = new SerializationRegistry();
        public INatsDeserialize<T> GetDeserializer<T>()
            => MessageSerializer<T>.Default;

        public INatsSerialize<T> GetSerializer<T>()
            => MessageSerializer<T>.Default;
    }
}
