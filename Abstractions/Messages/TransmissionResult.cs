namespace MQContract.Messages
{
    public record TransmissionResult(string ID,string? Error=null)
    {
        public bool IsError=>!string.IsNullOrWhiteSpace(Error);
    }
}
