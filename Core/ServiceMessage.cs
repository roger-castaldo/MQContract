using MQContract.ServiceAbstractions.Messages;

namespace MQContract
{
    internal record ServiceMessage
        : IServiceMessage
    {
        public string ID { get; private init; }
        public string Channel { get; private init; }
        public string MessageTypeID { get; private init; }
        public IMessageHeader Header { get; private init; }
        public ReadOnlyMemory<byte> Data { get; private init; }
        public ServiceMessage(Guid id, string messageTypeID, string channel, IMessageHeader header, byte[] data)
        {
            ID=$"{id}";
            Channel= channel;
            MessageTypeID= messageTypeID;
            Header= header;
            Data=data;
        }
    }
}
