using MQContract.KubeMQ.Interfaces;
using MQContract.Messages;

namespace MQContract.KubeMQ.Messages
{
    internal class PingResponse(MQContract.KubeMQ.SDK.Grpc.PingResult result,TimeSpan responseTime) : IKubeMQPingResult
    {
        public string Host => result.Host;

        public string Version => result.Version;

        public DateTime ServerStartTime => Utility.FromUnixTime(result.ServerStartTime);

        public TimeSpan ServerUpTime => TimeSpan.FromSeconds(result.ServerUpTimeSeconds);

        public TimeSpan ResponseTime => responseTime;
    }
}
