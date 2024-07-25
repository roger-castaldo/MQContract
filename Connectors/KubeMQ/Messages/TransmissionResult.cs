using MQContract.KubeMQ.SDK.Grpc;
using MQContract.Messages;

namespace MQContract.KubeMQ.Messages
{
    internal class TransmissionResult : ITransmissionResult
    {
        public TransmissionResult(Result res)
        {
            ID=res.EventID;
            Error=res.Error;
        }

        public TransmissionResult(string? messageID=null,string? error = null)
        {
            ID=messageID??string.Empty;
            Error=error;
        }

        public string ID { get; private init; }

        public bool IsError => !string.IsNullOrWhiteSpace(Error);

        public string? Error { get; private init; }
    }
}
