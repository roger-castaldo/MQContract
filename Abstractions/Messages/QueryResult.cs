namespace MQContract.Messages;

/// <summary>
/// Houses the result from a Query call into the system
/// </summary>
/// <typeparam name="TQueryResponse">The type of message in the response</typeparam>
/// <param name="ID">The unique ID of the message</param>
/// <param name="Header">The response headers</param>
/// <param name="Result">The resulting response if there was one</param>
/// <param name="Error">The error message for the response if it failed and an error was returned</param>
public record QueryResult<TQueryResponse>(string ID, MessageHeader Header, TQueryResponse? Result = default, ErrorMessage? Error = null)
    : TransmissionResult(ID, Error)
{ }
