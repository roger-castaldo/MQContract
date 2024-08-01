namespace MQContract.Messages
{
    /// <summary>
    /// Houses the result of a transmission into the system
    /// </summary>
    /// <param name="ID">The unique ID of the message that was transmitted</param>
    /// <param name="Error">An error message if an error occured</param>
    public record TransmissionResult(string ID,string? Error=null)
    {
        /// <summary>
        /// Flag to indicate if the result is an error
        /// </summary>
        public bool IsError=>!string.IsNullOrWhiteSpace(Error);
    }
}
