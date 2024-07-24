using MQContract.Messages;

namespace MQContract.KubeMQ.Messages
{
    internal class QueryResult : IServiceQueryResult
    {
        public QueryResult(string? id=null,string? error = null)
        {
            ID=id??string.Empty;
            Error=error;
        }

        public QueryResult(string messageID,MQContract.KubeMQ.SDK.Grpc.Response response)
        {
            ID = response.RequestID;
            MessageTypeID = response.Metadata;
            Channel = response.ReplyChannel;
            MessageID = messageID;
            Error = (!response.Executed && string.IsNullOrWhiteSpace(response.Error) ? "Failed to Execute" : response.Error);
            if (!IsError)
                Data = response.Body.ToByteArray();
            Header = new MessageHeader(response.Tags);
        }

        public string ID { get; private init; }

        public string MessageTypeID { get; private init; } = string.Empty;

        public string Channel { get; private init; } = string.Empty;

        public IMessageHeader Header { get; private init; } = new MessageHeader();

        public ReadOnlyMemory<byte> Data { get; private init; } = new();

        public string MessageID { get; private init; } = string.Empty;

        public bool IsError => !string.IsNullOrWhiteSpace(Error);

        public string? Error { get; private init; }
    }
}
