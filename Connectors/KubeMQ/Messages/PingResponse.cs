using MQContract.KubeMQ.Interfaces;
using MQContract.Messages;

namespace MQContract.KubeMQ.Messages
{
    internal record PingResponse
        : PingResult, IKubeMQPingResult
    {
        private readonly MQContract.KubeMQ.SDK.Grpc.PingResult result;
        public PingResponse(MQContract.KubeMQ.SDK.Grpc.PingResult result, TimeSpan responseTime)
            : base(result.Host, result.Version, responseTime) {
            this.result=result;
        }
        public DateTime ServerStartTime => Utility.FromUnixTime(result.ServerStartTime);

        public TimeSpan ServerUpTime => TimeSpan.FromSeconds(result.ServerUpTimeSeconds);

    }
}
