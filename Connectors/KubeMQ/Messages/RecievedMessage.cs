using Google.Protobuf;
using Google.Protobuf.Collections;
using MQContract.KubeMQ.SDK.Grpc;
using MQContract.Messages;

namespace MQContract.KubeMQ.Messages
{
    internal class RecievedMessage : IRecievedServiceMessage
    {
        public RecievedMessage(EventReceive evnt)
            : this(evnt.EventID, evnt.Metadata, evnt.Channel, evnt.Tags, evnt.Body) { }

        public RecievedMessage(Request request)
            : this(request.RequestID, request.Metadata, request.Channel, request.Tags, request.Body) { }

        private RecievedMessage(string id,string messageTypeID,string channel,MapField<string,string> header, ByteString data)
        {
            ID=id;
            MessageTypeID=messageTypeID;
            Channel=channel;
            Header = new MessageHeader(header);
            Data=data.ToArray();
        }
        public DateTime RecievedTimestamp { get; private init; } = DateTime.Now;

        public string ID { get; private init; }

        public string MessageTypeID { get; private init; }

        public string Channel { get; private init; }

        public IMessageHeader Header { get; private init; } 

        public ReadOnlyMemory<byte> Data { get; private init; } 
    }
}
