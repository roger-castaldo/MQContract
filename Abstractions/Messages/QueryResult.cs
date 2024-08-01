namespace MQContract.Messages
{
    /// <summary>
    /// Houses the result from a Query call into the system
    /// </summary>
    /// <typeparam name="T">The type of message in the response</typeparam>
    /// <param name="ID">The unique ID of the message</param>
    /// <param name="Header">The response headers</param>
    /// <param name="Result">The resulting response if there was one</param>
    /// <param name="Error">The error message for the response if it failed and an error was returned</param>
    public record QueryResult<T>(string ID,MessageHeader Header,T? Result=null,string? Error=null)
        : TransmissionResult(ID,Error)
        where T : class
    {}
}
