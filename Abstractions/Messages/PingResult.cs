namespace MQContract.Messages
{
    public record PingResult(string Host,string Version, TimeSpan ResponseTime)
    {}
}
