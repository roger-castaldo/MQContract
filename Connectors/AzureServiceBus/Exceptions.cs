namespace MQContract.AzureServiceBus;

/// <summary>
/// Thrown when a bulk publish request exceeds the usable message batch size
/// </summary>
public class BulkTooLargeException : ArgumentException
{
    internal BulkTooLargeException()
        : base("The bulk messages are too large for a batch.", "messages") { }
}
