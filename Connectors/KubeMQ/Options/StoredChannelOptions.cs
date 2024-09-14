using static MQContract.KubeMQ.Connection;

namespace MQContract.KubeMQ.Options
{
    internal record StoredChannelOptions(string ChannelName,MessageReadStyle ReadStyle=MessageReadStyle.StartNewOnly,long ReadOffset=0)
    {}
}
