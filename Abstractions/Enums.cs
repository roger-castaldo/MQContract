namespace MQContract
{
    /// <summary>
    /// These are the possible message filtering responses when supplying a message filtering action
    /// </summary>
    public enum MessageFilterResult
    {
        /// <summary>
        /// Allow the message to continue through
        /// </summary>
        Allow,
        /// <summary>
        /// Do not allow the message to continue to the callback and Acknowledge it within the service
        /// </summary>
        DropAndAcknowledge,
        /// <summary>
        /// Do not allow the message to continue to the callback and do not Acknowledge it within the service
        /// </summary>
        DropAndDontAcknowledge
    }
}
