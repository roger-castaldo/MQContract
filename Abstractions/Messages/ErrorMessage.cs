namespace MQContract.Messages
{
    /// <summary>
    /// Houses an Exception thrown that needs to be recorded as part of the tranmission result
    /// </summary>
    public record ErrorMessage
    {
        /// <summary>
        /// The exception that was thrown
        /// </summary>
        public Exception Exception { get; private init; }
        /// <summary>
        /// Used to indicate if it was Fatal or not.  If it is a Fatal exception, it will cause the Retry Policies to not be used
        /// </summary>
        public bool IsFatal { get; private init; }
        /// <summary>
        /// The error message from the exception
        /// </summary>
        public string Message => Exception.Message;

        /// <summary>
        /// Used to construct an instance of the Error object
        /// </summary>
        /// <param name="exception">The error that occured</param>
        /// <param name="isFatal">Used to indicate if the error was fatal.  There is also some additional checks inside that will set to Fatal if a given exception type is supplied.</param>
        /// <remarks>The exceptions that will override and mark fatal are ObjectDisposedException, ArgumentNullException, ArgumentOutOfRangeException, OperationCanceledException, InvalidOperationException</remarks>
        public ErrorMessage(Exception exception, bool isFatal = false)
        {
            if (exception is TransmissionException transmissionException)
            {
                exception = transmissionException.InnerException!;
                isFatal = transmissionException.IsFatal;
            }
            Exception = exception;
            IsFatal = isFatal || exception switch
            {
                ObjectDisposedException => true,
                ArgumentNullException => true,
                ArgumentOutOfRangeException => true,
                OperationCanceledException=>true,
                InvalidOperationException => true,
                TimeoutException => false,
                _ => false
            };
        }
    }
}
