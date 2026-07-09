namespace MQContract.Messages;

/// <summary>
/// Houses the result of a transmission into an underlying service with the corresponding name
/// </summary>
/// <param name="ServiceName">The unique name of the underlying service that was used to transmit</param>
/// <param name="Error">An error message if an error occured</param>
public record ChildTransmissionResult(string ServiceName, ErrorMessage? Error = null)
{
    /// <summary>
    /// Flag to indicate if the result is an error
    /// </summary>
    public bool IsError => !string.IsNullOrWhiteSpace(Error?.Message);
}

/// <summary>
/// Houses the result of a transmission into the system when using the MultiService method
/// </summary>
/// <param name="ID">The unique ID of the message that was transmitted</param>
/// <param name="Results">Houses all the results from each underlying system connection used</param>
public record MultiTransmissionResult(string ID, IEnumerable<ChildTransmissionResult> Results)
{
    /// <summary>
    /// Flag to indicate if there are any errors in the result
    /// </summary>
    public bool HasError => Results.Any(ctr => ctr.IsError);
}
