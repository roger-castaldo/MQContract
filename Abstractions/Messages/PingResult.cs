namespace MQContract.Messages
{
    /// <summary>
    /// Houses the results from a Ping call against a given underlying service
    /// </summary>
    /// <param name="Host">The host name of the service, if provided</param>
    /// <param name="Version">The version of the service running, if provided</param>
    /// <param name="ResponseTime">How long it took for the server to respond</param>
    public record PingResult(string Host,string Version, TimeSpan ResponseTime)
    {}
}
