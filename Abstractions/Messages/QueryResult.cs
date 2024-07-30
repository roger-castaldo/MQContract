namespace MQContract.Messages
{
    public record QueryResult<T>(string ID,IMessageHeader Header,T? Result=null,string? Error=null)
        : TransmissionResult(ID,Error)
        where T : class
    {}
}
