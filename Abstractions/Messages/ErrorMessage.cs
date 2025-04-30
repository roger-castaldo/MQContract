using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Messages
{
    public record ErrorMessage
    {
        public Exception Exception { get; private init; }
        public bool IsFatal { get; private init; }
        public string Message => Exception.Message;

        public ErrorMessage(Exception exception, bool isFatal = false)
        {
            if (exception is TransmissionException transmissionException)
            {
                exception = transmissionException.InnerException!;
                IsFatal = transmissionException.IsFatal;
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
