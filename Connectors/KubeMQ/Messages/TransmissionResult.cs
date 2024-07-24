using MQContract.KubeMQ.SDK.Grpc;
using MQContract.Messages;

namespace MQContract.KubeMQ.Messages
{
    internal class TransmissionResult : ITransmissionResult
    {
        public TransmissionResult(Result res)
        {
            MessageID=res.EventID;
            Error=res.Error;
        }

        public TransmissionResult(string? messageID=null,string? error = null)
        {
            MessageID=messageID??string.Empty;
            Error=error;
        }

        public string MessageID { get; private init; }

        public bool IsError => !string.IsNullOrWhiteSpace(Error);

        public string? Error { get; private init; }
    }
}
