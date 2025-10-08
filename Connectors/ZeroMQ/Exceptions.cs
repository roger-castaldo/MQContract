namespace MQContract.ZeroMQ
{
    /// <summary>
    /// Thrown when you have attempted to either create the inbox subscription or made a call to Query without setting up the inbox first
    /// </summary>
    public class UndefinedInboxException : Exception
    {
        internal UndefinedInboxException()
            : base("You must define the inbox connection address") { }

        internal static void ThrowIfNullOrWhiteSpace(string? inboxAddress)
        {
            if (string.IsNullOrWhiteSpace(inboxAddress))
                throw new UndefinedInboxException();
        }
    }
}
